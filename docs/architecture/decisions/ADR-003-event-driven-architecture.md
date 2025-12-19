# ADR-003: Event-Driven Architecture

**Date**: 2025-12-19  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

Our platform consists of multiple microservices (Competition Service, Judging Service, Analytics Service) that need to communicate and stay synchronized. For example:
- When an entry is paid → Judging Service needs to include it in flight assignments
- When a scoresheet is submitted → Competition Service needs to update progress tracking
- When bottles are checked in → Judging Service must validate physical availability

**Synchronous HTTP calls between services create tight coupling:**
- Services must all be available simultaneously
- Cascading failures (one service down = entire flow fails)
- Difficult to add new consumers (must modify producer)
- Hard to scale (producer must wait for consumer responses)

How do we enable **loosely-coupled, asynchronous communication** between services while guaranteeing:
- **Reliability**: No lost messages, even during service outages
- **Consistency**: Database writes and event publishing happen atomically
- **Scalability**: Multiple consumers can process events independently
- **Observability**: Track event flows through the system

---

## Decision Drivers

- **Loose Coupling**: Services should not know about each other's internal APIs
- **Resilience**: Service outages should not block event publishing
- **Scalability**: Easy to add new event consumers without modifying producers
- **Reliability**: Guaranteed message delivery (no lost events)
- **Consistency**: Events only published after database transaction commits
- **Observability**: Trace event flows across service boundaries
- **Standardization**: Vendor-neutral event format
- **Developer Experience**: Simple to publish and consume events

---

## Considered Options

### 1. Synchronous HTTP REST Calls
**Approach**: Services call each other's REST APIs directly

**Pros:**
- ✅ Simple request/response pattern
- ✅ Immediate feedback (errors returned in response)

**Cons:**
- ❌ **Tight coupling**: Producer must know all consumers
- ❌ **Cascading failures**: One service down breaks entire flow
- ❌ **Scalability limits**: Producer waits for all consumers
- ❌ **Hard to evolve**: Adding consumer requires producer changes

---

### 2. Event Bus (RabbitMQ) - Fire and Forget
**Approach**: Services publish events to RabbitMQ, consumers subscribe

**Pros:**
- ✅ Loose coupling (producer doesn't know consumers)
- ✅ Asynchronous (producer doesn't wait)
- ✅ Scalable (easy to add consumers)

**Cons:**
- ❌ **Dual-write problem**: DB write + RabbitMQ publish not atomic
- ❌ **Message loss risk**: Event published but DB rollback
- ❌ **Consistency**: Event published but transaction fails

---

### 3. Event Bus + Outbox Pattern
**Approach**: Write events to database table, background worker publishes to RabbitMQ

**Pros:**
- ✅ **Atomic**: Event stored in same DB transaction as entity changes
- ✅ **Reliable**: No message loss (events persisted in DB)
- ✅ **Eventual consistency**: Events published after commit
- ✅ **Retryable**: Failed publishes retried by worker

**Cons:**
- ❌ **Complexity**: Requires background worker and outbox table
- ❌ **Latency**: Slight delay between DB commit and event publish

---

### 4. Change Data Capture (CDC) with Debezium
**Approach**: Database triggers CDC events, Debezium streams to Kafka

**Pros:**
- ✅ No application code changes
- ✅ Guaranteed consistency (DB is source of truth)

**Cons:**
- ❌ **Kafka complexity**: Requires Kafka + ZooKeeper
- ❌ **Event structure**: Limited to DB row changes
- ❌ **Operational overhead**: CDC pipeline management

---

## Decision Outcome

**Chosen Option**: **#3 - Event Bus (RabbitMQ) + Outbox Pattern + CloudEvents Format**

---

## Implementation Details

### 1. RabbitMQ Configuration

#### Topic Exchange
Events routed by type (e.g., `competition.created`, `entry.submitted`):

```bash
# Exchange: competition_events (topic exchange)
Bindings:
  - Queue: judging_service_queue → Routing Key: entry.* (receives all entry events)
  - Queue: analytics_service_queue → Routing Key: *.* (receives all events)
```

#### Queue Settings
```csharp
// Durable queue with Dead Letter Queue (DLQ)
channel.QueueDeclare(
    queue: "judging_service_queue",
    durable: true,  // Survives broker restart
    exclusive: false,
    autoDelete: false,
    arguments: new Dictionary<string, object>
    {
        {"x-dead-letter-exchange", "dlx_exchange"},  // Failed messages go here
        {"x-message-ttl", 86400000}  // 24-hour TTL
    });
```

---

### 2. Outbox Pattern Implementation

#### Database Schema
```sql
CREATE TABLE event_store (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    aggregate_id UUID NOT NULL,  -- Entity ID (competition_id, entry_id)
    event_type VARCHAR(100) NOT NULL,  -- e.g., "entry.submitted"
    event_data JSONB NOT NULL,  -- CloudEvents payload
    created_at TIMESTAMP DEFAULT NOW(),
    published_at TIMESTAMP NULL,  -- NULL = not yet published
    published_status VARCHAR(20) DEFAULT 'PENDING',  -- PENDING | PUBLISHED | FAILED
    retry_count INT DEFAULT 0,
    
    INDEX idx_event_store_pending (published_status, created_at) 
        WHERE published_status = 'PENDING'
);
```

#### Command Handler (Atomic Write)
```csharp
public class CreateEntryHandler : IRequestHandler<CreateEntryCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEventMapper _eventMapper;

    public async Task<Result<Guid>> Handle(
        CreateEntryCommand cmd, 
        CancellationToken ct)
    {
        // 1. Create domain entity
        var entry = Entry.Create(cmd.CompetitionId, cmd.BrewerName, cmd.StyleId);

        // 2. Persist entity
        await _dbContext.Entries.AddAsync(entry, ct);

        // 3. Store event in Outbox (same transaction)
        var cloudEvent = _eventMapper.MapToCloudEvent(
            eventType: "com.beercomp.entry.submitted",
            source: "/services/competition",
            data: new
            {
                tenant_id = entry.TenantId,
                competition_id = entry.CompetitionId,
                entry_id = entry.Id,
                judging_number = entry.JudgingNumber,
                style_id = entry.StyleId
            });

        await _dbContext.EventStore.AddAsync(new EventStoreEntry
        {
            TenantId = entry.TenantId,
            AggregateId = entry.Id,
            EventType = "entry.submitted",
            EventData = JsonSerializer.Serialize(cloudEvent),
            PublishedStatus = "PENDING"
        }, ct);

        // 4. Commit transaction (atomic: entity + event)
        await _dbContext.SaveChangesAsync(ct);

        return Result.Success(entry.Id);
    }
}
```

#### Background Worker (Event Publisher)
```csharp
public class EventPublisherWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRabbitMQPublisher _publisher;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Fetch pending events (batch of 100)
            var pendingEvents = await dbContext.EventStore
                .Where(e => e.PublishedStatus == "PENDING")
                .OrderBy(e => e.CreatedAt)
                .Take(100)
                .ToListAsync(stoppingToken);

            // 2. Publish to RabbitMQ
            foreach (var evt in pendingEvents)
            {
                try
                {
                    await _publisher.PublishAsync(
                        exchange: "competition_events",
                        routingKey: evt.EventType,
                        message: evt.EventData);

                    // 3. Mark as published
                    evt.PublishedStatus = "PUBLISHED";
                    evt.PublishedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    // 4. Mark as failed (retry with exponential backoff)
                    evt.RetryCount++;
                    evt.PublishedStatus = evt.RetryCount > 5 ? "FAILED" : "PENDING";
                    _logger.LogError(ex, "Failed to publish event {EventId}", evt.Id);
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);

            // Poll every 5 seconds
            await Task.Delay(5000, stoppingToken);
        }
    }
}
```

---

### 3. CloudEvents 1.0 Format

Standard event structure for interoperability:

```json
{
  "specversion": "1.0",
  "type": "com.beercomp.entry.submitted",
  "source": "/services/competition",
  "id": "a89f3c7e-8b1a-4c2f-9d3e-5f6a7b8c9d0e",
  "time": "2025-12-19T10:30:00Z",
  "datacontenttype": "application/json",
  "data": {
    "tenant_id": "123e4567-e89b-12d3-a456-426614174000",
    "competition_id": "789e4567-e89b-12d3-a456-426614174999",
    "entry_id": "456e4567-e89b-12d3-a456-426614174888",
    "judging_number": 42,
    "style_id": "21A",
    "brewer_name": "[REDACTED]",  // Anonymous during judging
    "entry_name": "West Coast IPA",
    "special_ingredients": "Cascade, Centennial hops"
  }
}
```

**Key Events:**
| Event Type | Producer | Consumers | Purpose |
|------------|----------|-----------|---------|
| `competition.created` | Competition Service | Analytics | Track competition metrics |
| `entry.submitted` | Competition Service | - | Entry registered |
| `entry.paid` | Competition Service | Judging Service | Include in flight assignments |
| `bottles.checked_in` | Competition Service | Judging Service | Validate physical availability |
| `flight.created` | Judging Service | Competition Service | Update judging progress |
| `scoresheet.submitted` | Judging Service | Competition Service | Aggregate scores |
| `consensus.completed` | Judging Service | Competition Service | Final score determined |
| `bos.completed` | Judging Service | Competition Service | Best of Show winner |

---

### 4. Event Consumer (Judging Service)

```csharp
public class EntryPaidEventHandler : IEventHandler<EntryPaidEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public async Task HandleAsync(EntryPaidEvent evt, CancellationToken ct)
    {
        // 1. Validate event (idempotency check)
        var alreadyProcessed = await _dbContext.ProcessedEvents
            .AnyAsync(e => e.EventId == evt.Id, ct);
        
        if (alreadyProcessed)
        {
            _logger.LogInformation("Event {EventId} already processed", evt.Id);
            return;  // Idempotent
        }

        // 2. Business logic: Add entry to eligible pool
        await _dbContext.EligibleEntries.AddAsync(new EligibleEntry
        {
            TenantId = evt.Data.TenantId,
            CompetitionId = evt.Data.CompetitionId,
            EntryId = evt.Data.EntryId,
            JudgingNumber = evt.Data.JudgingNumber,
            StyleId = evt.Data.StyleId
        }, ct);

        // 3. Record event as processed
        await _dbContext.ProcessedEvents.AddAsync(new ProcessedEvent
        {
            EventId = evt.Id,
            EventType = evt.Type,
            ProcessedAt = DateTime.UtcNow
        }, ct);

        await _dbContext.SaveChangesAsync(ct);
    }
}
```

---

## Guarantees

### Reliability
✅ **At-Least-Once Delivery**: Events persisted in DB before publishing (outbox)  
✅ **No Lost Messages**: Failed publishes retried by background worker  
✅ **Idempotency**: Consumers track processed events to avoid duplicates  
✅ **Dead Letter Queue**: Failed messages after 5 retries moved to DLQ  

### Consistency
✅ **Atomicity**: Event stored in same transaction as entity changes  
✅ **Eventual Consistency**: Events published within seconds of DB commit  
✅ **Order Preservation**: Events processed in FIFO order per aggregate  

### Observability
✅ **Distributed Tracing**: OpenTelemetry traces event flows across services  
✅ **Event Log**: All events stored in `event_store` table (audit trail)  
✅ **Monitoring**: Track event processing latency, retry counts, DLQ size  

---

## Consequences

### Positive
✅ **Loose Coupling**: Services only depend on event contracts  
✅ **Resilience**: Service outages don't block event publishing  
✅ **Scalability**: Easy to add new consumers without producer changes  
✅ **Reliability**: Outbox pattern guarantees no message loss  
✅ **Observability**: CloudEvents provide standardized structure for tracing  
✅ **Evolvability**: Event versioning supports backward compatibility  

### Negative
❌ **Eventual Consistency**: Slight delay (seconds) between DB commit and event delivery  
❌ **Complexity**: Requires background worker and outbox table  
❌ **Debugging**: Harder to trace than synchronous calls (mitigated with OpenTelemetry)  
❌ **Storage Overhead**: Events stored in DB increase storage costs  

### Risks
⚠️ **Event Schema Changes**: Breaking changes require versioning strategy  
⚠️ **Background Worker Failure**: Events stuck in outbox (mitigated with monitoring alerts)  
⚠️ **RabbitMQ Downtime**: Events queue in outbox (processed when broker recovers)  
⚠️ **Duplicate Processing**: Consumers must implement idempotency  

---

## Alternatives Considered

### Why not synchronous HTTP?
- ❌ **Tight coupling**: Producer must know all consumers
- ❌ **Cascading failures**: One service down breaks entire flow
- ❌ **Hard to scale**: Producer waits for all consumers

### Why not fire-and-forget RabbitMQ?
- ❌ **Dual-write problem**: DB write + event publish not atomic
- ❌ **Message loss**: Event published but DB transaction rollback

### Why not Kafka + CDC?
- ❌ **Overkill**: Kafka operational complexity for our event volume
- ❌ **Event structure**: Limited to DB row changes (not rich domain events)
- ❌ **Cost**: Kafka resource-intensive

### Why not Azure Service Bus?
- ✅ **RabbitMQ chosen** for vendor-agnostic portability
- ✅ **Open-source** enables local development without Azure costs
- ✅ **Topic exchanges** more flexible than Service Bus topics

---

## Testing Strategy

### Unit Tests (Event Handlers)
```csharp
[Fact]
public async Task EntryPaidEvent_AddsToEligiblePool()
{
    // Arrange
    var evt = new EntryPaidEvent
    {
        Id = Guid.NewGuid(),
        Data = new { entry_id = EntryId, judging_number = 42 }
    };

    // Act
    await _handler.HandleAsync(evt, CancellationToken.None);

    // Assert
    var eligibleEntry = await _dbContext.EligibleEntries
        .FirstAsync(e => e.EntryId == EntryId);
    eligibleEntry.JudgingNumber.Should().Be(42);
}
```

### Integration Tests (Testcontainers + RabbitMQ)
```csharp
[Fact]
public async Task OutboxWorker_PublishesEventsToRabbitMQ()
{
    // Arrange: Insert pending event in DB
    await _dbContext.EventStore.AddAsync(new EventStoreEntry
    {
        EventType = "entry.submitted",
        EventData = JsonSerializer.Serialize(cloudEvent),
        PublishedStatus = "PENDING"
    });
    await _dbContext.SaveChangesAsync();

    // Act: Run background worker once
    await _worker.ProcessBatchAsync();

    // Assert: Event marked as published
    var evt = await _dbContext.EventStore.FirstAsync();
    evt.PublishedStatus.Should().Be("PUBLISHED");
    evt.PublishedAt.Should().NotBeNull();

    // Assert: Message in RabbitMQ
    var message = await _rabbitMQTestClient.ConsumeAsync("judging_service_queue");
    message.Should().Contain("entry.submitted");
}
```

---

## Related Decisions

- [ADR-001: Tech Stack Selection](ADR-001-tech-stack-selection.md) (RabbitMQ choice)
- [ADR-005: CQRS Pattern Implementation](ADR-005-cqrs-implementation.md) (Command handlers publish events)

---

## References

- [RabbitMQ Reliability Guide](https://www.rabbitmq.com/reliability.html)
- [Outbox Pattern (Microservices.io)](https://microservices.io/patterns/data/transactional-outbox.html)
- [CloudEvents Specification](https://cloudevents.io/)
- [OpenTelemetry Distributed Tracing](https://opentelemetry.io/docs/concepts/signals/traces/)
- [Idempotent Event Processing](https://exactly-once.github.io/posts/exactly-once-delivery/)
