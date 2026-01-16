# BFF API Gateway

Backend-for-Frontend (BFF) API Gateway for the Beer Competition SaaS Platform.

## Overview

The BFF serves as the single entry point for the frontend application (React PWA), handling:
- **Authentication**: JWT token validation with Keycloak
- **Token Exchange**: OAuth 2.0 Token Exchange (RFC 8693) for service-specific tokens
- **Authorization**: Role-based access control (RBAC)
- **Multi-tenancy**: Tenant isolation enforcement
- **Routing**: Reverse proxy to backend microservices
- **Connection Management**: Optimized HTTP client with connection pooling and HTTP/2 support
- **Observability**: Distributed tracing and structured logging

## Architecture

```
Frontend (PWA)
      ↓
   (JWT with aud: bff-api)
      ↓
BFF API Gateway (this project)
      ↓
   Token Exchange
      ↓
   (JWT with aud: service-name)
      ↓
   YARP Reverse Proxy
      ↓
Competition Service | Judging Service
```

### Key Responsibilities

1. **Authentication** (ADR-004):
   - Validates JWT tokens from Keycloak (audience: `bff-api`)
   - Extracts `tenant_id`, `sub` (user ID), and `roles` claims
   - Returns 401 Unauthorized for invalid tokens

2. **Token Exchange** (RFC 8693):
   - Exchanges frontend tokens for service-specific tokens
   - Each downstream service receives token with its own audience
   - Prevents token reuse across service boundaries (zero-trust security)
   - See: `docs/architecture/SERVICE_AUDIENCES_TOKEN_EXCHANGE.md`

3. **Authorization** (ADR-004):
   - Enforces role-based policies: Organizer, Judge, Entrant, Steward
   - Route-level authorization (e.g., competitions require Organizer role)
   - Returns 403 Forbidden for insufficient permissions

3. **Multi-Tenancy** (ADR-002):
   - Extracts `tenant_id` from JWT claims
   - Injects `X-Tenant-ID` header to all downstream requests
   - Ensures data isolation at the gateway level

4. **Resilience**:
   - **Connection Pooling**: YARP reuses HTTP connections for up to 5 minutes
   - **HTTP/2 Support**: Multiple streams per connection for better performance
   - **Service-Level Resilience**: Downstream services (Competition, Judging) implement their own circuit breakers, retries, and timeouts
   - **Future Enhancement**: Gateway-level resilience can be added via custom `IForwarderHttpClientFactory` if needed

5. **Observability** (ADR-003):
   - Distributed tracing with OpenTelemetry
   - Structured logging with Serilog (JSON format)
   - Correlation ID propagation (`X-Correlation-ID`)
   - Health check endpoint: `/health`

## Configuration

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `Keycloak__Authority` | Keycloak realm URL | `http://localhost:8080/realms/beercomp` |
| `Keycloak__Audience` | JWT audience claim for BFF | `bff-api` |
| `Keycloak__ClientId` | BFF client ID for token exchange | `bff-api` |
| `Keycloak__ClientSecret` | BFF client secret for token exchange | `<secret>` |
| `Keycloak__TokenEndpoint` | Keycloak token endpoint | `http://localhost:8080/realms/beercomp/protocol/openid-connect/token` |
| `ServiceClients__CompetitionService__Audience` | Competition Service audience | `competition-service` |
| `ServiceClients__JudgingService__Audience` | Judging Service audience | `judging-service` |
| `Services__Competition` | Competition Service URL | `http://localhost:5001` |
| `Services__Judging` | Judging Service URL | `http://localhost:5002` |
| `Cors__AllowedOrigins__0` | Frontend origin | `http://localhost:5173` |

### appsettings.json Structure

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/beercomp",
    "Audience": "bff-api",
    "ClientId": "bff-api",
    "ClientSecret": "<secret>",
    "TokenEndpoint": "http://localhost:8080/realms/beercomp/protocol/openid-connect/token",
    "RequireHttpsMetadata": false
  },
  "ServiceClients": {
    "CompetitionService": {
      "ClientId": "competition-service",
      "Audience": "competition-service"
    },
    "JudgingService": {
      "ClientId": "judging-service",
      "Audience": "judging-service"
    }
  },
  "Services": {
    "Competition": "http://localhost:5001",
    "Judging": "http://localhost:5002"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

## Token Exchange Flow

### OAuth 2.0 Token Exchange (RFC 8693)

1. Frontend authenticates with Keycloak → receives JWT with `aud: "bff-api"`
2. Frontend calls BFF API with this token
3. BFF validates token (audience check)
4. BFF exchanges token for service-specific token:
   ```http
   POST /realms/beercomp/protocol/openid-connect/token
   
   grant_type=urn:ietf:params:oauth:grant-type:token-exchange
   &subject_token=<frontend_token>
   &audience=competition-service
   ```
5. Keycloak returns JWT with `aud: "competition-service"`
6. BFF forwards request to service with exchanged token
7. Service validates token (audience + signature)

**Benefits:**
- **Audience Isolation**: Each service only accepts its own audience
- **Zero Trust**: Tokens cannot be reused across services
- **Least Privilege**: Each service gets only required claims

## API Routes

All routes require authentication (JWT Bearer token).

### Competition Service Routes
- `GET/POST /api/competitions/**` → Competition Service
- `GET/POST/PUT /api/entries/**` → Competition Service

**Authorization**: 
- Competitions: Organizer role required
- Entries: Authenticated users (all roles)

### Judging Service Routes
- `GET /api/flights/**` → Judging Service
- `GET/POST /api/scoresheets/**` → Judging Service

**Authorization**:
- Flights: Judge or Organizer role required
- Scoresheets: Judge role required

**Authorization**: Judge or Organizer roles only

## Running Locally

### Prerequisites
- .NET 10 SDK
- Keycloak running (see `infrastructure/docker-compose.yml`)
- Competition Service running on port 5001
- Judging Service running on port 5002 (when available)

### Start the BFF

```bash
cd backend/BFF.ApiGateway
dotnet run
```

The BFF will start on `https://localhost:7038`.

### Health Check

```bash
curl https://localhost:7038/health
```

Expected response: `200 OK` with status details.

## Testing

### Run Unit Tests

```bash
cd backend
dotnet test BFF.ApiGateway.Tests/BeerCompetition.BFF.ApiGateway.Tests.csproj
```

### Test with JWT Token

1. Obtain JWT token from Keycloak:
   ```bash
   curl -X POST http://localhost:8080/realms/beercomp/protocol/openid-connect/token \
     -d "client_id=bff-api" \
     -d "client_secret=<your-secret>" \
     -d "grant_type=password" \
     -d "username=<username>" \
     -d "password=<password>"
   ```

2. Call BFF with token:
   ```bash
   curl -H "Authorization: Bearer <token>" \
     https://localhost:7038/api/competitions
   ```

3. Verify headers in downstream service logs:
   - `X-Tenant-ID`: Should match `tenant_id` claim
   - `X-User-ID`: Should match `sub` claim
   - `X-Correlation-ID`: Generated or propagated

## Middleware Pipeline

The request pipeline executes in this order:

1. **CorrelationIdMiddleware**: Generates/propagates correlation ID
2. **Serilog Request Logging**: Logs request details
3. **CORS**: Validates origin
4. **Authentication**: Validates JWT token
5. **Authorization**: Checks role-based policies
6. **TenantExtractionMiddleware**: Injects `X-Tenant-ID` header
7. **YARP Reverse Proxy**: Routes to downstream services

## Authorization Policies

| Policy | Required Roles | Usage |
|--------|---------------|-------|
| `AuthenticatedUser` | Any authenticated | All routes |
| `Organizer` | Organizer | Competition management |
| `Judge` | Judge | Scoresheet submission |
| `Entrant` | Entrant | Entry registration |
| `Steward` | Steward | Bottle check-in |
| `JudgeOrOrganizer` | Judge, Organizer | Flight/scoresheet access |
| `OrganizerOrSteward` | Organizer, Steward | Logistics management |

## HTTP Client Configuration

The BFF configures YARP's HTTP client with optimized settings:

### Connection Management
- **Connection Lifetime**: 5 minutes (prevents stale connections)
- **Idle Timeout**: 2 minutes (releases unused connections)
- **HTTP/2**: Multiple streams per connection enabled
- **Proxy**: Direct connection to services (no intermediary proxy)

### Resilience Strategy

**Service-Level Resilience** (Recommended):
- Downstream services (Competition Service, Judging Service) implement their own resilience patterns
- Each service has circuit breakers, retries, and timeouts appropriate to its domain
- This provides better granularity and control over failure scenarios

**Gateway-Level Resilience** (Future Enhancement):
- Can be implemented using a custom `IForwarderHttpClientFactory`
- Would wrap YARP's `HttpMessageInvoker` with Polly policies
- Consider this if cross-cutting resilience requirements emerge

## Observability

### Structured Logging

Logs include:
- Timestamp, log level, message
- Correlation ID
- User ID and Tenant ID (when authenticated)
- Request path, method, status code
- Duration

### Distributed Tracing

OpenTelemetry spans:
- `AspNetCore` instrumentation: HTTP requests
- `HttpClient` instrumentation: Downstream calls
- Custom spans: Middleware execution

View traces in console (development) or export to OTLP collector (production).

## Security Considerations

⚠️ **NEVER commit secrets to source control**

- Store Keycloak client secret in environment variables or Azure Key Vault
- Use HTTPS in production (`RequireHttpsMetadata: true`)
- Validate JWT issuer and audience claims
- Enforce short token lifetimes (5-15 minutes)
- Rotate signing keys regularly

## Troubleshooting

### 401 Unauthorized
- Verify JWT token is valid: Check expiration, issuer, audience
- Ensure Keycloak Authority URL is correct
- Check token includes required claims: `tenant_id`, `sub`, `roles`

### 403 Forbidden (Missing tenant_id)
- JWT token must include `tenant_id` claim
- Configure Keycloak client mapper to include `tenant_id`

### 503 Service Unavailable
- Downstream service (Competition/Judging) is down or unreachable
- Check service health endpoints
- Review service logs for errors

### CORS Errors
- Verify frontend origin is in `Cors:AllowedOrigins` configuration
- Check browser console for specific CORS error

## Related Documentation

- **ADR-004**: [Authentication & Authorization](../../docs/architecture/decisions/ADR-004-authentication-authorization.md)
- **ADR-002**: [Multi-Tenancy Strategy](../../docs/architecture/decisions/ADR-002-multi-tenancy-strategy.md)
- **ADR-003**: [Event-Driven Architecture](../../docs/architecture/decisions/ADR-003-event-driven-architecture.md)

## Issue Tracking

This implementation resolves [Issue #67](https://github.com/jesuscorral/beer-competition-saas/issues/67).

---

**Branch**: `67-bff-api-gateway`  
**Status**: ✅ Complete - Ready for PR
