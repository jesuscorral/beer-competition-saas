# BFF API Gateway

Backend-for-Frontend (BFF) API Gateway for the Beer Competition SaaS Platform.

## Overview

The BFF serves as the single entry point for the frontend application (React PWA), handling:
- **Authentication**: JWT token validation with Keycloak
- **Authorization**: Role-based access control (RBAC)
- **Multi-tenancy**: Tenant isolation enforcement
- **Routing**: Reverse proxy to backend microservices
- **Resilience**: Circuit breaker and retry policies
- **Observability**: Distributed tracing and structured logging

## Architecture

```
Frontend (PWA)
      ↓
BFF API Gateway (this project)
      ↓
   YARP Reverse Proxy
      ↓
Competition Service | Judging Service
```

### Key Responsibilities

1. **Authentication** (ADR-004):
   - Validates JWT tokens from Keycloak
   - Extracts `tenant_id`, `sub` (user ID), and `roles` claims
   - Returns 401 Unauthorized for invalid tokens

2. **Authorization** (ADR-004):
   - Enforces role-based policies: Organizer, Judge, Entrant, Steward
   - Route-level authorization (e.g., scoresheets require Judge or Organizer role)
   - Returns 403 Forbidden for insufficient permissions

3. **Multi-Tenancy** (ADR-002):
   - Extracts `tenant_id` from JWT claims
   - Injects `X-Tenant-ID` header to all downstream requests
   - Ensures data isolation at the gateway level

4. **Resilience**:
   - Circuit breaker: Opens after 50% failure rate (5+ requests in 10s)
   - Retry policy: 3 attempts with exponential backoff
   - Timeout: 30 seconds per request

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
| `Keycloak__Audience` | JWT audience claim | `bff-api` |
| `Services__Competition` | Competition Service URL | `http://localhost:5001` |
| `Services__Judging` | Judging Service URL | `http://localhost:5002` |
| `Cors__AllowedOrigins__0` | Frontend origin | `http://localhost:5173` |

### appsettings.json Structure

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/beercomp",
    "Audience": "bff-api",
    "RequireHttpsMetadata": false
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

## API Routes

All routes require authentication (JWT Bearer token).

### Competition Service Routes
- `GET/POST /api/competitions/**` → Competition Service
- `GET/POST/PUT /api/entries/**` → Competition Service

**Authorization**: Authenticated users (all roles)

### Judging Service Routes
- `GET /api/flights/**` → Judging Service
- `GET/POST /api/scoresheets/**` → Judging Service

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

The BFF will start on `http://localhost:5190`.

### Health Check

```bash
curl http://localhost:5190/health
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
     http://localhost:5190/api/competitions
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

## Resilience Configuration

### Circuit Breaker
- **Sampling Duration**: 10 seconds
- **Failure Ratio**: 50%
- **Minimum Throughput**: 5 requests
- **Break Duration**: 30 seconds

### Retry Policy
- **Max Attempts**: 3
- **Backoff**: Exponential with jitter
- **Total Timeout**: 30 seconds

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

### 503 Service Unavailable (Circuit Breaker Open)
- Downstream service is down or slow
- Check Competition/Judging Service health
- Circuit breaker will close after 30 seconds if service recovers

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
