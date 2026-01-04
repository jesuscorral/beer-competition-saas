# ADR-010: OAuth 2.0 Token Exchange for Service-Specific Audiences

**Date**: 2026-01-04  
**Status**: Accepted  
**Deciders**: Architecture Team, Security Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

In a microservices architecture with a **Backend-for-Frontend (BFF)** acting as API Gateway, we need to secure service-to-service communication. The frontend obtains a token from Keycloak (OIDC provider), but:

**Problems:**
- **Token Reuse**: A single token with `aud: "bff-api"` can be used to call any backend service
- **Security Risk**: If a token is stolen from network traffic, it can access all services
- **Lack of Audience Isolation**: Services cannot verify tokens are intended for them
- **No Least Privilege**: Frontend token has all permissions for all services

**Question**: How do we implement **zero-trust security** between BFF and microservices, ensuring each service validates tokens with **service-specific audiences**?

---

## Decision Drivers

- **Zero Trust Security**: Services must independently validate tokens
- **Audience Isolation**: Tokens must be scoped to specific services
- **Token Reuse Prevention**: Stolen tokens cannot be used across service boundaries
- **Standards Compliance**: Use industry-standard OAuth 2.0 protocols
- **Defense in Depth**: Multiple layers of security validation
- **Developer Experience**: Transparent token exchange (no manual intervention)
- **Scalability**: Support adding new microservices easily

---

## Considered Options

### 1. Single Token for All Services
**Approach**: Frontend token with `aud: "bff-api"` is forwarded to all services

**Pros:**
- ✅ Simple implementation
- ✅ No additional token exchange

**Cons:**
- ❌ **No audience isolation**: Any valid token can access any service
- ❌ **High security risk**: Stolen token compromises all services
- ❌ **No zero-trust**: Services blindly trust BFF tokens

---

### 2. BFF Signs Service-Specific Tokens
**Approach**: BFF validates frontend token, then signs new JWTs for each service

**Pros:**
- ✅ Service-specific audiences
- ✅ BFF controls token generation

**Cons:**
- ❌ **BFF is single point of failure**: Compromised BFF can forge tokens
- ❌ **Key management burden**: BFF must securely store signing keys
- ❌ **Non-standard**: Custom token signing logic

---

### 3. OAuth 2.0 Token Exchange (RFC 8693)
**Approach**: BFF requests service-specific tokens from Keycloak using token exchange

**Pros:**
- ✅ **Standards-based**: RFC 8693 specification
- ✅ **Audience isolation**: Each service gets dedicated token
- ✅ **Zero-trust**: Keycloak is single source of truth
- ✅ **Defense in depth**: Services independently validate
- ✅ **Automatic token caching**: BFF caches exchanged tokens
- ✅ **Keycloak native**: Built-in support in Keycloak

**Cons:**
- ❌ **Additional latency**: Token exchange adds ~10-50ms per request (mitigated by caching)
- ❌ **Keycloak dependency**: Requires Keycloak as OIDC provider

---

## Decision Outcome

**Chosen Option**: **#3 - OAuth 2.0 Token Exchange (RFC 8693)**

---

## Token Exchange Flow

```
┌─────────────┐
│  Frontend   │  1. Login with frontend-spa client
│    (SPA)    │  ───────────────────────────►
└─────────────┘                              │
                                             ▼
                                    ┌────────────────┐
                                    │   Keycloak     │
                                    │  (OIDC IdP)    │
                                    └────────────────┘
                                             │
       ┌─────────────────────────────────────┘
       │ 2. JWT with aud: "bff-api"
       ▼
┌─────────────┐
│     BFF     │  3. API Request with token
│  (Gateway)  │  ◄───────────────────────────
└─────────────┘
       │
       │ 4. Token Exchange:
       │    POST /realms/beercomp/protocol/openid-connect/token
       │    grant_type=urn:ietf:params:oauth:grant-type:token-exchange
       │    subject_token=<frontend_token>
       │    requested_token_type=urn:ietf:params:oauth:token-type:access_token
       │    audience=<service-name>-service
       │
       ▼
┌──────────────────┐
│  Competition     │  5. Validate service-specific token
│  Service         │     (aud: competition-service)
└──────────────────┘
```

---

## Implementation

### Keycloak Clients

Four clients with specific audiences:

1. **`frontend-spa`** (Public with PKCE): React SPA authentication, tokens target BFF
2. **`bff-api`** (Confidential): BFF validates frontend tokens and exchanges them, token exchange enabled
3. **`competition-service`** (Bearer-Only): Competition service validates tokens with `competition-service` audience
4. **`judging-service`** (Bearer-Only): Judging service validates tokens with `judging-service` audience

### Configuration Overview

**BFF Configuration (`appsettings.json`):**
- Keycloak authority and audience (`bff-api`)
- Client credentials for token exchange
- Service client mappings (competition-service, judging-service)

**BFF Services:**
- JWT authentication middleware validates frontend tokens
- `ITokenExchangeService` exchanges tokens with Keycloak
- YARP transforms inject exchanged tokens into proxied requests
- Token caching (5-minute buffer before expiration)

**Service Configuration:**
- Each service validates JWT with service-specific audience
- Authorization policies enforce role-based access
- `[Authorize]` attributes on controllers/endpoints

### Token Exchange Flow

1. Frontend obtains JWT from Keycloak with `aud: "bff-api"`
2. Frontend calls BFF with token in Authorization header
3. BFF validates token (audience = `bff-api`)
4. BFF exchanges token with Keycloak:
   - `grant_type: urn:ietf:params:oauth:grant-type:token-exchange`
   - `subject_token: <frontend_token>`
   - `audience: <target_service>` (e.g., `competition-service`)
5. Keycloak returns new JWT with `aud: "competition-service"`
6. BFF proxies request to service with exchanged token
7. Service validates token (audience = `competition-service`)

**For detailed implementation examples**, see [SERVICE_AUDIENCES_TOKEN_EXCHANGE.md](../SERVICE_AUDIENCES_TOKEN_EXCHANGE.md)

---

## Security Benefits

### 1. Audience Isolation
- **Before**: Any valid token could access any service
- **After**: Each service validates `aud` claim matches its client ID (`competition-service`, `judging-service`)
- **Result**: Tokens stolen from one service cannot be used on another

### 2. Zero Trust
- **Before**: Services blindly trusted tokens from BFF
- **After**: Each service independently validates signature + audience with Keycloak
- **Result**: Defense in depth - compromised BFF can't forge service tokens

### 3. Least Privilege
- **Before**: Single token with all permissions for all services
- **After**: BFF requests minimal scopes per service
- **Result**: Reduced blast radius if token is compromised

### 4. Token Reuse Prevention
- **Before**: Token stolen from network traffic could be replayed anywhere
- **After**: Audience claim ties token to specific service
- **Result**: Stolen token only works for intended service

---

## Performance Considerations

### Token Exchange Latency
- **Overhead**: ~10-50ms per token exchange call to Keycloak
- **Mitigation**: Token caching in BFF (5-minute expiration buffer)
- **Impact**: First request per user per service has latency, subsequent requests hit cache

### Cache Strategy
```csharp
// Cache key: "exchanged_token:{subject_token}:{target_audience}"
// Expiration: token_expires_in - 300 seconds (5-minute buffer)
_cache.Set(cacheKey, exchangedToken, TimeSpan.FromSeconds(expiresIn - 300));
```

---

## Service Implementation Checklist

When creating a **new microservice**, follow these steps:

### 1. Keycloak Client Configuration
1. Create bearer-only client: `<service-name>-service`
2. Add audience protocol mapper: `audience-<service-name>`
3. Configure mapper to include `<service-name>-service` in `aud` claim
4. Generate and save client secret to `.env` file

### 2. BFF Configuration
Update `appsettings.json`:
```json
{
  "ServiceClients": {
    "<ServiceName>Service": {
      "ClientId": "<service-name>-service",
      "Audience": "<service-name>-service"
    }
  },
  "ReverseProxy": {
    "Routes": {
      "<service-name>-route": {
        "ClusterId": "<service-name>-cluster",
        "Match": { "Path": "/api/<service-name>/{**catch-all}" }
      }
    },
    "Clusters": {
      "<service-name>-cluster": {
        "Destinations": {
          "destination1": { "Address": "http://localhost:<port>" }
        }
      }
    }
  }
}
```

### 3. Service Authentication
Configure JWT authentication with service-specific audience:
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/beercomp";
        options.Audience = "<service-name>-service";  // CRITICAL
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true  // CRITICAL
        };
    });
```

### 4. Apply [Authorize] Attribute
```csharp
[Authorize]
public class MyController : ControllerBase { ... }
```

---

## Consequences

### Positive
✅ **Zero-trust security**: Each service independently validates tokens  
✅ **Audience isolation**: Service-specific tokens prevent cross-service reuse  
✅ **Standards-based**: RFC 8693 OAuth 2.0 Token Exchange  
✅ **Defense in depth**: Multiple validation layers  
✅ **Scalable**: Easy to add new services  
✅ **Automatic caching**: BFF caches exchanged tokens for performance  

### Negative
❌ **Keycloak dependency**: Requires Keycloak as OIDC provider  
❌ **Additional latency**: Token exchange adds ~10-50ms (mitigated by caching)  
❌ **Increased complexity**: More moving parts in authentication flow  
❌ **Cache invalidation**: Need strategy for token revocation  

### Risks
⚠️ **Token exchange failures**: If Keycloak unavailable, BFF cannot exchange tokens  
⚠️ **Cache poisoning**: Must secure cached tokens (in-memory cache acceptable for MVP)  
⚠️ **Clock skew**: Services must have synchronized clocks for token expiration validation  

---

## Alternatives Considered

### Why not BFF signs tokens?
- ❌ **Single point of failure**: Compromised BFF can forge tokens
- ❌ **Key management burden**: BFF must securely store signing keys
- ✅ **Token Exchange**: Keycloak is single source of truth

### Why not single token for all services?
- ❌ **No audience isolation**: Stolen token compromises all services
- ❌ **No zero-trust**: Services blindly trust BFF
- ✅ **Token Exchange**: Service-specific audiences enforce isolation

---

## Related Decisions

- [ADR-004: Authentication & Authorization](ADR-004-authentication-authorization.md) - Base authentication strategy
- [ADR-009: Modular Monolith with Vertical Slices](ADR-009-modular-monolith-vertical-slices.md) - Service boundaries

---

## References

- **RFC 8693**: OAuth 2.0 Token Exchange - https://datatracker.ietf.org/doc/html/rfc8693
- **Keycloak Token Exchange**: https://www.keycloak.org/docs/latest/securing_apps/#_token-exchange
- **YARP Reverse Proxy**: https://microsoft.github.io/reverse-proxy/
- **Implementation Guide**: [SERVICE_AUDIENCES_TOKEN_EXCHANGE.md](../SERVICE_AUDIENCES_TOKEN_EXCHANGE.md)

---

**Last Updated**: 2026-01-04  
**Implementation Status**: ✅ Complete (BFF + Competition Service + Judging Service)  
**Tested**: Yes (Local Development Environment)
