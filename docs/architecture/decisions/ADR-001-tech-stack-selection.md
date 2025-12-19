# ADR-001: Tech Stack Selection

**Date**: 2025-12-19  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

We need to select a comprehensive technology stack for a multi-tenant SaaS platform that manages BJCP-compliant homebrew beer competitions. The platform must support:
- 200+ entrants per competition
- 50+ concurrent judges with offline scoresheet entry
- 600+ bottles with tracking and conflict-of-interest enforcement
- Real-time scoring aggregation
- Multi-tenant data isolation
- Future ML-driven analytics

Which technologies will best support these requirements while maintaining development velocity, operational stability, and scalability?

---

## Decision Drivers

- **Performance**: Handle 50+ concurrent judges submitting scoresheets
- **Multi-tenancy**: Strict data isolation with Row-Level Security
- **Offline Capability**: Judges need to work without internet connectivity
- **Developer Experience**: Modern tooling, strong typing, active ecosystem
- **Scalability**: Horizontal scaling for peak competition periods
- **Event-Driven**: Asynchronous communication between services
- **Observability**: Distributed tracing and monitoring
- **Cost**: Open-source first, Azure-native where appropriate

---

## Considered Options

### Backend Options
1. **.NET 10 (C#)** - Modern, high-performance, enterprise-ready
2. **Java (Spring Boot)** - Mature ecosystem, JVM performance
3. **Node.js (NestJS)** - JavaScript everywhere, reactive patterns
4. **Go** - High performance, concurrency primitives

### Database Options
1. **PostgreSQL** - Advanced features (RLS, JSONB, full-text search)
2. **SQL Server** - .NET native, Azure SQL integration
3. **MongoDB** - Document model flexibility
4. **CockroachDB** - Distributed SQL with multi-region

### Message Bus Options
1. **RabbitMQ** - Mature, robust, feature-rich
2. **Azure Service Bus** - Managed service, Azure-native
3. **Kafka** - High throughput, event streaming
4. **NATS** - Lightweight, cloud-native

### Frontend Options
1. **React** - Large ecosystem, component reusability
2. **Vue.js** - Progressive framework, simpler learning curve
3. **Angular** - Full-featured, TypeScript-native
4. **Svelte** - Compiler-based, smaller bundle sizes

---

## Decision Outcome

**Chosen Options:**

### Backend
- **.NET 10 (C#)** for Competition Service, Judging Service, BFF/API Gateway
- **Python 3.12 + FastAPI** for Analytics Service (Post-MVP)

**Rationale:**
- **.NET 10** provides excellent performance, strong typing, mature dependency injection, and Entity Framework Core for database access
- **MediatR** library enables clean CQRS implementation
- **C# async/await** simplifies asynchronous programming
- **Python** ideal for data science/ML workloads (judge assignment optimization, anomaly detection)
- **FastAPI** provides Python's best REST API framework with automatic OpenAPI generation

### Database
- **PostgreSQL 17+** with Row-Level Security (RLS)

**Rationale:**
- **Row-Level Security** provides database-enforced multi-tenancy (security-in-depth)
- **JSONB columns** for flexible storage of scoresheet details
- **Full-text search** for competition/entry lookup
- **Mature ecosystem** with Entity Framework Core provider
- **Open-source** with excellent Azure managed offering (Azure Database for PostgreSQL)

### Message Bus
- **RabbitMQ**

**Rationale:**
- **Topic exchanges** enable flexible event routing
- **Durable queues** guarantee message delivery
- **Dead Letter Queue (DLQ)** for failed message handling
- **Open-source** with Azure-native option (Azure Service Bus RabbitMQ support)
- **Mature .NET client** (RabbitMQ.Client, MassTransit)
- **Lower complexity** than Kafka for our event volume

### Frontend
- **React 18 + TypeScript**
- **Progressive Web App (PWA)** with Service Workers + IndexedDB
- **TanStack Query** for server state management
- **Zustand** for client state management
- **Tailwind CSS** for styling

**Rationale:**
- **React's ecosystem** is unmatched (component libraries, tooling, community)
- **TypeScript** provides type safety and better DX
- **PWA capabilities** enable offline scoresheet entry (critical for judges in venues with poor connectivity)
- **TanStack Query** handles caching, background refetching, optimistic updates
- **IndexedDB** stores scoresheets locally until connectivity restored
- **Tailwind CSS** enables rapid, mobile-first responsive design

### Observability
- **OpenTelemetry** for distributed tracing
- **Azure Monitor / Application Insights** for metrics and logs

**Rationale:**
- **OpenTelemetry** is vendor-neutral, future-proof standard
- **Azure Monitor** provides excellent integration with Azure-hosted services
- **Distributed tracing** essential for debugging microservices

---

## Consequences

### Positive
✅ **.NET performance** handles high concurrency with low latency  
✅ **PostgreSQL RLS** provides database-enforced multi-tenancy  
✅ **RabbitMQ reliability** ensures no event loss  
✅ **React PWA** enables offline judge workflows  
✅ **Python analytics** leverages ML ecosystem (scikit-learn, pandas)  
✅ **TypeScript** across stack improves type safety  
✅ **OpenTelemetry** provides observability without vendor lock-in  

### Negative
❌ **Two backend languages** (.NET + Python) requires polyglot skillset  
❌ **RabbitMQ operational complexity** compared to managed Azure Service Bus  
❌ **React bundle size** larger than Svelte/Vue (mitigated with code splitting)  
❌ **PWA limitations** on iOS (IndexedDB quota, Service Worker restrictions)  

### Risks
⚠️ **PostgreSQL scaling limits**: Single-instance vertical scaling (mitigated: read replicas for analytics)  
⚠️ **RabbitMQ single point of failure**: Cluster setup required for HA  
⚠️ **Offline PWA sync conflicts**: Requires robust conflict resolution (last-write-wins with timestamps)  

---

## Alternatives Considered

### Why not Java/Spring Boot?
- **.NET performance** matches or exceeds JVM in benchmarks
- **C# language features** (records, pattern matching) more modern than Java
- **Entity Framework Core** simpler than JPA/Hibernate for our use cases
- **Team expertise** stronger in .NET

### Why not Azure Service Bus?
- **RabbitMQ portability** avoids vendor lock-in
- **Open-source** enables local development without Azure emulators
- **Topic exchanges** more flexible than Service Bus topics
- **Cost**: Self-hosted RabbitMQ cheaper at scale

### Why not MongoDB?
- **Multi-tenancy** requires application-level filtering (error-prone)
- **PostgreSQL RLS** provides database-enforced isolation (security-in-depth)
- **Relational model** better fits competition/entry/scoresheet relationships
- **JSONB** provides document flexibility where needed

### Why not Kafka?
- **Overkill** for our event volume (<1000 events/min peak)
- **Operational complexity** (ZooKeeper, partition management)
- **RabbitMQ simpler** for request/reply patterns
- **Cost**: Kafka resource-intensive

---

## Related Decisions

- [ADR-002: Multi-Tenancy Strategy](ADR-002-multi-tenancy-strategy.md)
- [ADR-003: Event-Driven Architecture](ADR-003-event-driven-architecture.md)
- [ADR-007: Frontend Architecture](ADR-007-frontend-architecture.md)

---

## References

- [.NET Benchmarks](https://www.techempower.com/benchmarks/)
- [PostgreSQL Row-Level Security](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [RabbitMQ Reliability Guide](https://www.rabbitmq.com/reliability.html)
- [PWA Offline Patterns](https://web.dev/offline-cookbook/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
