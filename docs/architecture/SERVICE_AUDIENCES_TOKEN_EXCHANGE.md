# Service-Specific Audiences & Token Exchange

**Date**: 2025-12-25  
**Status**: Implemented  
**Context**: Authentication & Authorization Enhancement

---

## Overview

This project implements **service-specific audiences** and **OAuth 2.0 Token Exchange (RFC 8693)** to enforce **zero-trust security** between microservices. Each service validates tokens with its own audience claim, preventing token reuse across service boundaries.

---

## Architecture

### Token Flow

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
       │    - Validate frontend token (aud: bff-api)
       │    - Exchange for service token
       │    - Keycloak returns JWT with target audience
       │
       ▼
┌──────────────────┐
│  Competition     │  5. Validate service-specific token
│  Service         │     (aud: competition-service)
└──────────────────┘
```

---

## Keycloak Configuration

### Clients

Four clients with specific audiences:

#### 1. `bff-api` (Confidential)
- **Purpose**: BFF/API Gateway validates frontend tokens
- **Audience**: `bff-api`
- **Token Exchange**: Enabled (`oauth2.token.exchange.grant.enabled: true`)
- **Client Secret**: `KEYCLOAK_BFF_CLIENT_SECRET`

#### 2. `competition-service` (Bearer-Only)
- **Purpose**: Competition microservice validates service tokens
- **Audience**: `competition-service`
- **Bearer-Only**: Yes (no login flows, only token validation)
- **Client Secret**: `KEYCLOAK_COMPETITION_CLIENT_SECRET`

#### 3. `judging-service` (Bearer-Only)
- **Purpose**: Judging microservice validates service tokens
- **Audience**: `judging-service`
- **Bearer-Only**: Yes
- **Client Secret**: `KEYCLOAK_JUDGING_CLIENT_SECRET`

#### 4. `frontend-spa` (Public with PKCE)
- **Purpose**: React SPA authentication
- **Audience**: `bff-api` (frontend tokens target BFF)
- **PKCE**: S256 (code challenge method)
- **Public Client**: Yes (no client secret)

---

## Implementation

### BFF Configuration

**appsettings.json:**
```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/beercomp",
    "Audience": "bff-api",
    "ClientId": "bff-api",
    "ClientSecret": "${KEYCLOAK_BFF_CLIENT_SECRET}",
    "TokenEndpoint": "http://localhost:8080/realms/beercomp/protocol/openid-connect/token"
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
  }
}
```

### Token Exchange Service

**Implementation**: `KeycloakTokenExchangeService.cs`

```csharp
public async Task<string> ExchangeTokenAsync(
    string subjectToken,
    string targetAudience,
    CancellationToken cancellationToken = default)
{
    var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "grant_type", "urn:ietf:params:oauth:grant-type:token-exchange" },
        { "client_id", _clientId },
        { "client_secret", _clientSecret },
        { "subject_token", subjectToken },
        { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
        { "requested_token_type", "urn:ietf:params:oauth:token-type:access_token" },
        { "audience", targetAudience }
    });

    var response = await _httpClient.PostAsync(_tokenEndpoint, requestContent, cancellationToken);
    // ... handle response
}
```

### YARP Transform

**Implementation**: `TokenExchangeTransform.cs`

Automatically exchanges tokens before forwarding to downstream services:

```csharp
public override async ValueTask ApplyAsync(RequestTransformContext context)
{
    var originalToken = ExtractBearerToken(context.HttpContext.Request.Headers.Authorization);
    
    var exchangedToken = await _tokenExchangeService.ExchangeTokenAsync(
        originalToken,
        _targetAudience,
        context.HttpContext.RequestAborted);

    context.ProxyRequest.Headers.Authorization = 
        new AuthenticationHeaderValue("Bearer", exchangedToken);
}
```

---

## Authorization Policies

### BFF Policies (Route-Level)

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUser", policy => 
        policy.RequireAuthenticatedUser());
    
    options.AddPolicy("Organizer", policy => 
        policy.RequireRole("organizer"));
    
    options.AddPolicy("Judge", policy => 
        policy.RequireRole("judge"));
    
    options.AddPolicy("Entrant", policy => 
        policy.RequireRole("entrant"));
    
    options.AddPolicy("Steward", policy => 
        policy.RequireRole("steward"));
    
    options.AddPolicy("JudgeOrOrganizer", policy => 
        policy.RequireRole("judge", "organizer"));
    
    options.AddPolicy("OrganizerOrSteward", policy => 
        policy.RequireRole("organizer", "steward"));
});
```

### Route Configuration

```json
{
  "ReverseProxy": {
    "Routes": {
      "competition-route": {
        "ClusterId": "competition-cluster",
        "AuthorizationPolicy": "Organizer",
        "Match": { "Path": "/api/competitions/{**catch-all}" }
      },
      "entries-route": {
        "ClusterId": "competition-cluster",
        "AuthorizationPolicy": "AuthenticatedUser",
        "Match": { "Path": "/api/entries/{**catch-all}" }
      },
      "flights-route": {
        "ClusterId": "judging-cluster",
        "AuthorizationPolicy": "JudgeOrOrganizer",
        "Match": { "Path": "/api/flights/{**catch-all}" }
      },
      "scoresheets-route": {
        "ClusterId": "judging-cluster",
        "AuthorizationPolicy": "Judge",
        "Match": { "Path": "/api/scoresheets/{**catch-all}" }
      }
    }
  }
}
```

---

## Service Implementation Checklist

When creating a **new microservice**, follow these steps to implement audience validation:

### 1. Keycloak Client Configuration

```json
{
  "clientId": "<service-name>-service",
  "bearerOnly": true,
  "serviceAccountsEnabled": true,
  "protocolMappers": [
    {
      "name": "audience-<service-name>",
      "protocolMapper": "oidc-audience-mapper",
      "config": {
        "included.client.audience": "<service-name>-service",
        "access.token.claim": "true"
      }
    }
  ]
}
```

### 2. Environment Variables

Add to `.env` file:
```bash
KEYCLOAK_<SERVICE_NAME>_CLIENT_SECRET=<generate-secure-secret>
```

### 3. BFF Configuration

Update `appsettings.json`:
```json
{
  "ServiceClients": {
    "<ServiceName>Service": {
      "ClientId": "<service-name>-service",
      "Audience": "<service-name>-service"
    }
  }
}
```

### 4. Service Authentication Configuration

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/beercomp";
        options.Audience = "<service-name>-service";  // Service-specific audience
        options.RequireHttpsMetadata = false;  // Development only
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,  // CRITICAL: Validates audience claim
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "roles"
        };
    });
```

### 5. Apply [Authorize] to Controllers/Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Require authentication
public class CompetitionsController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "organizer")]  // Role-based authorization
    public async Task<IActionResult> CreateCompetition(...)
    {
        // Only organizers with valid competition-service token can call
    }
}
```

---

## Security Benefits

### 1. Audience Isolation
- **Problem**: Without audiences, any valid token can access any service
- **Solution**: Each service validates `aud` claim matches its client ID
- **Result**: Tokens stolen from one service cannot be used on another

### 2. Zero Trust
- **Problem**: Services blindly trust tokens from BFF
- **Solution**: Each service independently validates signature + audience
- **Result**: Defense in depth - compromised BFF can't forge service tokens

### 3. Least Privilege
- **Problem**: Single token with all permissions for all services
- **Solution**: BFF requests minimal scopes per service
- **Result**: Reduced blast radius if token is compromised

### 4. Token Reuse Prevention
- **Problem**: Token stolen from network traffic can be replayed
- **Solution**: Audience claim ties token to specific service
- **Result**: Stolen token only works for intended service

---

## Testing Token Exchange

### 1. Obtain Frontend Token

```bash
# Get token from Keycloak
curl -X POST "http://localhost:8080/realms/beercomp/protocol/openid-connect/token" \
  -d "client_id=frontend-spa" \
  -d "username=organizer@beercomp.local" \
  -d "password=organizer123" \
  -d "grant_type=password"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 900
}
```

**Decode token** (jwt.io):
```json
{
  "aud": "bff-api",
  "sub": "user-uuid",
  "tenant_id": "11111111-1111-1111-1111-111111111111",
  "roles": ["organizer"]
}
```

### 2. Call BFF API

```bash
curl -X GET "http://localhost:5190/api/competitions" \
  -H "Authorization: Bearer <frontend_token>"
```

### 3. Verify Token Exchange

BFF logs should show:
```
[DEBUG] Exchanging token for audience competition-service
[DEBUG] Successfully exchanged token for audience competition-service, expires in 900 seconds
```

### 4. Verify Service Receives Correct Token

Competition Service logs should validate:
```json
{
  "aud": "competition-service",  // ✅ Correct audience
  "sub": "user-uuid",
  "tenant_id": "11111111-1111-1111-1111-111111111111",
  "roles": ["organizer"]
}
```

---

## Troubleshooting

### Error: "The audience 'bff-api' is invalid"

**Cause**: Service expects `competition-service` audience but receives `bff-api`

**Fix**: Ensure YARP transform is configured to exchange tokens:
```csharp
.AddTokenExchangeTransforms(configuration)
```

### Error: "Token exchange failed with status 400"

**Cause**: Keycloak client not configured for token exchange

**Fix**: Add to client configuration:
```json
{
  "attributes": {
    "oauth2.token.exchange.grant.enabled": "true"
  }
}
```

### Error: "IDX10214: Audience validation failed"

**Cause**: Service audience configuration mismatch

**Fix**: Verify `appsettings.json`:
```json
{
  "Keycloak": {
    "Audience": "competition-service"  // Must match Keycloak client ID
  }
}
```

---

## References

- **RFC 8693**: OAuth 2.0 Token Exchange - https://datatracker.ietf.org/doc/html/rfc8693
- **Keycloak Documentation**: Token Exchange - https://www.keycloak.org/docs/latest/securing_apps/#_token-exchange
- **ADR-004**: Authentication & Authorization - `docs/architecture/decisions/ADR-004-authentication-authorization.md`
- **Keycloak Setup Guide**: `docs/setup/KEYCLOAK_SETUP.md`

---

## Roles (No Admin Role)

**Four realm roles defined:**
- `organizer`: Competition organizer - creates competitions, manages entries
- `judge`: BJCP certified judge - views flights, submits scoresheets
- `entrant`: Competition entrant - registers beers, pays fees, views results
- `steward`: Competition steward - checks in bottles, manages logistics

**Note**: Admin role has been removed per project requirements - four roles are sufficient for MVP.

---

## Next Steps for New Services

When implementing **new microservices** (e.g., Payment Service, Notification Service):

1. **Create Keycloak client** (bearer-only) with service-specific audience mapper
2. **Add client secret** to `.env` file
3. **Update BFF configuration** with new service client mapping
4. **Configure JWT authentication** in new service with correct audience
5. **Add YARP route** with appropriate authorization policy
6. **Test token exchange** flow end-to-end
7. **Update documentation** with service-specific notes

---

**Last Updated**: 2025-12-25  
**Implementation Status**: ✅ Complete (BFF + Documentation)  
**Tested**: Yes (Local Development Environment)
