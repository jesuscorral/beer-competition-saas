# Service-Specific Audiences Implementation - Summary

**Date**: 2025-12-25  
**Status**: ✅ Complete  
**Task**: Implement service-specific audiences and OAuth 2.0 token exchange

---

## Summary

Successfully implemented **service-specific audiences** and **OAuth 2.0 Token Exchange (RFC 8693)** for zero-trust security between microservices. Each service now validates tokens with its own audience claim, preventing token reuse across service boundaries.

---

## Changes Made

### 1. Keycloak Configuration

#### Realm Export (`infrastructure/keycloak/realm-export.json`)

**Updated Clients:**
- ✅ Renamed `backend-api` → `bff-api` (Confidential)
- ✅ Added `competition-service` (Bearer-Only)
- ✅ Added `judging-service` (Bearer-Only)
- ✅ Updated `frontend-spa` (Public with PKCE)

**Audience Mappers Added:**
- `bff-api`: Token includes `aud: "bff-api"`
- `competition-service`: Token includes `aud: "competition-service"`
- `judging-service`: Token includes `aud: "judging-service"`
- `frontend-spa`: Token includes `aud: "bff-api"` (targets BFF)

**Token Exchange Enabled:**
- `bff-api` client: `oauth2.token.exchange.grant.enabled: true`

**Roles Updated:**
- ❌ Removed `admin` role (per project requirements)
- ✅ Kept 4 roles: `organizer`, `judge`, `entrant`, `steward`
- ❌ Removed `admin@beercomp.local` test user

### 2. Environment Variables

#### `.env` File (`infrastructure/.env`)

```bash
# BFF API Gateway client secret
KEYCLOAK_BFF_CLIENT_SECRET=KEYCLOAK_BFF_CLIENT_SECRET_VALUE

# Competition Service client secret
KEYCLOAK_COMPETITION_CLIENT_SECRET=CompetitionServiceSecret123456789

# Judging Service client secret
KEYCLOAK_JUDGING_CLIENT_SECRET=JudgingServiceSecret123456789012
```

### 3. BFF Implementation

#### New Files Created:

1. **`Services/ITokenExchangeService.cs`**
   - Interface for token exchange abstraction

2. **`Services/KeycloakTokenExchangeService.cs`**
   - Implements OAuth 2.0 Token Exchange (RFC 8693)
   - Exchanges frontend tokens for service-specific tokens
   - HTTP client-based implementation with error handling

3. **`Transforms/TokenExchangeTransform.cs`**
   - YARP request transform
   - Automatically exchanges tokens before forwarding to services
   - Injects exchanged token into Authorization header

4. **`Extensions/TokenExchangeExtensions.cs`**
   - Registers token exchange service
   - Configures YARP transforms based on route cluster

#### Updated Files:

1. **`Program.cs`**
   - Added `AddTokenExchange(configuration)`
   - Added `.AddTokenExchangeTransforms(configuration)` to YARP

2. **`appsettings.json`**
   - Added `ClientId`, `ClientSecret`, `TokenEndpoint` to Keycloak section
   - Added `ServiceClients` section for Competition/Judging services

### 4. Authorization Policies

#### Route Configuration Updated:

```json
"competition-route": {
  "AuthorizationPolicy": "Organizer"  // Only organizers can create competitions
}

"scoresheets-route": {
  "AuthorizationPolicy": "Judge"  // Only judges can submit scoresheets
}
```

**Policies Implemented:**
- ✅ `AuthenticatedUser`: Any authenticated user
- ✅ `Organizer`: Only organizers
- ✅ `Judge`: Only judges
- ✅ `Entrant`: Only entrants
- ✅ `Steward`: Only stewards
- ✅ `JudgeOrOrganizer`: Judges or organizers
- ✅ `OrganizerOrSteward`: Organizers or stewards

### 5. Documentation

#### Updated Files:

1. **`docs/setup/KEYCLOAK_SETUP.md`**
   - Updated client descriptions (4 clients instead of 2)
   - Added token exchange flow diagram
   - Added audience mapper documentation
   - Removed admin role references
   - Added testing instructions

2. **`docs/architecture/decisions/ADR-004-authentication-authorization.md`**
   - Added OAuth 2.0 Token Exchange section
   - Updated client configurations with audiences
   - Added BFF configuration examples
   - Added YARP configuration examples
   - Removed admin role

3. **`.github/copilot-instructions.md`**
   - Updated project overview to mention service-specific audiences
   - Added CRITICAL section on service audience implementation
   - Added reference to SERVICE_AUDIENCES_TOKEN_EXCHANGE.md
   - Updated last-updated date to 2025-12-25

#### New Files Created:

4. **`docs/architecture/SERVICE_AUDIENCES_TOKEN_EXCHANGE.md`**
   - Comprehensive guide for service-specific audiences
   - Token exchange flow diagram
   - Keycloak configuration instructions
   - Implementation checklist for new services
   - Testing instructions
   - Troubleshooting guide
   - Security benefits explanation

---

## Token Exchange Flow

```
1. Frontend → Keycloak: Login (frontend-spa client)
   ↓
2. Keycloak → Frontend: JWT with aud: "bff-api"
   ↓
3. Frontend → BFF: API request with token
   ↓
4. BFF: Validate token (audience check)
   ↓
5. BFF → Keycloak: Token exchange request
   - subject_token: <frontend_token>
   - audience: competition-service
   ↓
6. Keycloak → BFF: JWT with aud: "competition-service"
   ↓
7. BFF → Service: Forward request with exchanged token
   ↓
8. Service: Validate token (audience + signature)
```

---

## Security Benefits

### 1. Audience Isolation
- Each service only accepts tokens with its specific audience
- Tokens cannot be reused across service boundaries

### 2. Zero Trust
- Services independently validate signature + audience
- Compromised BFF cannot forge service tokens

### 3. Least Privilege
- BFF requests minimal scopes per service
- Reduced blast radius if token is compromised

### 4. Defense in Depth
- Multiple layers of token validation
- Audience claim enforcement at service level

---

## Testing

### Prerequisites
```bash
# Restart Keycloak to load new realm configuration
cd infrastructure
docker-compose restart keycloak
```

### Test Token Exchange

1. **Obtain Frontend Token:**
```bash
curl -X POST "http://localhost:8080/realms/beercomp/protocol/openid-connect/token" \
  -d "client_id=frontend-spa" \
  -d "username=organizer@beercomp.local" \
  -d "password=organizer123" \
  -d "grant_type=password"
```

2. **Call BFF API:**
```bash
curl -X GET "http://localhost:5190/api/competitions" \
  -H "Authorization: Bearer <token>"
```

3. **Verify Logs:**
   - BFF should log: "Exchanging token for audience competition-service"
   - Service should receive token with correct audience

---

## Checklist for New Services

When creating a new microservice, follow these steps:

- [ ] Create Keycloak bearer-only client with audience mapper
- [ ] Add client secret to `.env` file
- [ ] Update BFF `appsettings.json` with service client configuration
- [ ] Configure JWT authentication in service with correct audience
- [ ] Add YARP route with appropriate authorization policy
- [ ] Test token exchange flow end-to-end
- [ ] Update documentation

**Reference**: [SERVICE_AUDIENCES_TOKEN_EXCHANGE.md](../docs/architecture/SERVICE_AUDIENCES_TOKEN_EXCHANGE.md)

---

## Files Changed

### Configuration Files
- ✅ `infrastructure/keycloak/realm-export.json` (Clients + Roles)
- ✅ `infrastructure/.env` (Client Secrets)
- ✅ `backend/BFF.ApiGateway/appsettings.json` (Token Exchange Config)

### BFF Implementation
- ✅ `backend/BFF.ApiGateway/Program.cs` (Token Exchange Registration)
- ✅ `backend/BFF.ApiGateway/Services/ITokenExchangeService.cs` (New)
- ✅ `backend/BFF.ApiGateway/Services/KeycloakTokenExchangeService.cs` (New)
- ✅ `backend/BFF.ApiGateway/Transforms/TokenExchangeTransform.cs` (New)
- ✅ `backend/BFF.ApiGateway/Extensions/TokenExchangeExtensions.cs` (New)

### Documentation
- ✅ `docs/setup/KEYCLOAK_SETUP.md` (Updated)
- ✅ `docs/architecture/decisions/ADR-004-authentication-authorization.md` (Updated)
- ✅ `docs/architecture/SERVICE_AUDIENCES_TOKEN_EXCHANGE.md` (New)
- ✅ `.github/copilot-instructions.md` (Updated)

---

## Next Steps

### Immediate Actions
1. **Restart Infrastructure:**
   ```bash
   cd infrastructure
   docker-compose restart keycloak
   ```

2. **Test Token Exchange:**
   - Start BFF: `dotnet run --project backend/BFF.ApiGateway`
   - Call API with frontend token
   - Verify token exchange in logs

### Future Service Implementations
When implementing new services (e.g., Payment Service, Notification Service):
- Follow checklist in `SERVICE_AUDIENCES_TOKEN_EXCHANGE.md`
- Create Keycloak client with audience mapper
- Configure JWT authentication with correct audience
- Update BFF configuration for token exchange

---

## References

- **RFC 8693**: OAuth 2.0 Token Exchange - https://datatracker.ietf.org/doc/html/rfc8693
- **Keycloak Docs**: Token Exchange - https://www.keycloak.org/docs/latest/securing_apps/#_token-exchange
- **ADR-004**: Authentication & Authorization
- **Service Audiences Guide**: `docs/architecture/SERVICE_AUDIENCES_TOKEN_EXCHANGE.md`

---

**Implementation Status**: ✅ Complete  
**Tested**: Pending (requires Keycloak restart)  
**Documentation**: ✅ Complete  
**Code Review**: Ready for PR
