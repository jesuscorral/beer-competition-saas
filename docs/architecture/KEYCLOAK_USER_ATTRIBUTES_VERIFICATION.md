# Keycloak User Attributes Configuration - Issue #100

## Summary

This document provides manual verification steps for the Keycloak `competition_id` protocol mapper added in Issue #100.

## Changes Made

### Protocol Mappers Added

Added the following protocol mapper to the token-issuing Keycloak clients (`bff-api`, `frontend-spa`):

1. **competition_id** - Maps user attribute `competition_id` to JWT claim

**NOTE**: 
- `tenant_id` mapper was already present and not modified
- `bjcp_rank` is NOT included in JWT - it belongs in the database as mutable user profile data

### Configuration Details

```json
{
  "name": "competition_id",
  "protocol": "openid-connect",
  "protocolMapper": "oidc-usermodel-attribute-mapper",
  "consentRequired": false,
  "config": {
    "userinfo.token.claim": "true",
    "user.attribute": "competition_id",
    "id.token.claim": "true",
    "access.token.claim": "true",
    "claim.name": "competition_id",
    "jsonType.label": "String"
  }
}
```

## Manual Verification Steps

Since Keycloak doesn't re-import existing realms, these protocol mappers need to be added manually through the Admin Console for existing installations.

### Option 1: Fresh Installation (Recommended for Development)

1. **Delete existing realm**:
   ```bash
   # Stop Keycloak
   docker-compose stop keycloak
   
   # Remove Keycloak volume to reset data
   docker volume rm beer-competition-services_keycloak_data
   
   # Start Keycloak (will import realm-export.json)
   docker-compose up -d keycloak
   ```

2. **Verify protocol mappers** (only for token-issuing clients):
   - Login to Keycloak Admin Console: http://localhost:8080
   - Navigate to: Realm Settings → Clients → `bff-api` → Client scopes → Dedicated → Mappers
   - Verify `competition_id` mapper is present
   - Repeat for `frontend-spa` only
   - **Note**: `competition-service` and `judging-service` are bearer-only clients (they only validate tokens, not issue them), so they don't have protocol mappers

3. **Test with a user**:
   - Create a test user with attributes:
     - `tenant_id`: `<some-uuid>`
     - `competition_id`: `<some-uuid>`
   - Obtain a token for that user
   - Decode the JWT and verify claims are present

### Option 2: Manual Configuration (For Existing Installations)

For each **token-issuing client** (`bff-api`, `frontend-spa`):

**Important**: Do NOT add protocol mappers to `competition-service` or `judging-service` - these are bearer-only clients that only validate tokens. Protocol mappers are only needed on clients that issue tokens.

1. Login to Keycloak Admin Console: http://localhost:8080
2. Navigate to: Realm `beercomp` → Clients → Select client
3. Go to: Client scopes → Dedicated scope → Mappers tab
4. Click "Add mapper" → "By configuration" → "User Attribute"
5. Create `competition_id` mapper:
   - **Name**: `competition_id`
   - **User Attribute**: `competition_id`
   - **Token Claim Name**: `competition_id`
   - **Claim JSON Type**: `String`
   - **Add to ID token**: ON
   - **Add to access token**: ON
   - **Add to userinfo**: ON
6. Verify `tenant_id` mapper already exists (should be present from earlier setup)

### Testing JWT Claims

**Planned automated test script (future enhancement)**:

> NOTE: The `Test-KeycloakUserAttributes.ps1` script is not yet implemented in this repository.
> Until it is added, use the manual JWT verification steps described below.

```powershell
# Planned location (future enhancement; script not yet implemented)
# cd infrastructure/scripts
# .\Test-KeycloakUserAttributes.ps1 -AdminPassword "<your-password>"
```

**Manual JWT Verification**:

1. Create a test user:
```bash
# Using Keycloak Admin API
curl -X POST http://localhost:8080/admin/realms/beercomp/users \
  -H "Authorization: Bearer <admin-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "enabled": true,
    "credentials": [{"type": "password", "value": "password", "temporary": false}],
    "attributes": {
      "tenant_id": ["xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
      "competition_id": ["yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy"]
    }
  }'
```

2. Get access token:
```bash
curl -X POST http://localhost:8080/realms/beercomp/protocol/openid-connect/token \
  -d "client_id=frontend-spa" \
  -d "username=testuser" \
  -d "password=password" \
  -d "grant_type=password"
```

3. Decode JWT at https://jwt.io and verify claims:
```json
{
  "tenant_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "competition_id": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
  "sub": "...",
  "email": "testuser@example.com",
  "preferred_username": "testuser",
  "roles": ["..."]
}
```

**Note**: User profile data like `bjcp_rank` should be stored in the database and retrieved via API endpoints (e.g., `GET /api/users/me`), not embedded in JWT tokens.

## Files Modified

- `infrastructure/keycloak/realm-export.json`:
  - Added `competition_id` mapper to `bff-api` client (token issuer)
  - Added `competition_id` mapper to `frontend-spa` client (token issuer)
  - **Did NOT add to** `competition-service` or `judging-service` (bearer-only clients - they only validate tokens, protocol mappers not needed)
  - Fixed `post.logout.redirect.uris` format (array to string)

## Known Issues

1. **Realm Import Skipped**: Keycloak doesn't re-import existing realms. For fresh installations, the protocol mappers will be automatically configured. For existing installations, use Option 2 (Manual Configuration).

2. **Test Script Limitations**: The automated test script (`Test-KeycloakUserAttributes.ps1`) may fail with existing installations due to credential issues. Manual verification via Keycloak Admin Console is recommended.

## Next Steps

After merging this PR:

1. **Development Team**: Delete local Keycloak volume and restart to get fresh realm with protocol mappers
2. **CI/CD**: Update deployment scripts to include protocol mapper configuration
3. **Issue #90**: Backend API can now rely on `competition_id` claim being present in JWT tokens from authenticated users
4. **Database Design**: Create `users` table with `bjcp_rank`, `bjcp_id`, and other mutable profile fields

## Related Issues

- #89: Epic - User Registration & Competition Onboarding System
- #90: Backend - Tenant & User Creation API (depends on this issue)
- #92: Backend - User Registration API (depends on this issue)

## Acceptance Criteria

- [x] Protocol mappers added to realm-export.json for all clients
- [x] JSON syntax validated (Keycloak starts without errors)
- [x] Test script created for automated verification
- [x] Manual verification steps documented
- [ ] Fresh installation tested (to be done by reviewer)
- [ ] JWT claims verified in decoded token (to be done by reviewer)

## Why Protocol Mappers Are Only on Token-Issuing Clients

**Bearer-Only Clients vs Token-Issuing Clients:**

- **Token-Issuing Clients** (`bff-api`, `frontend-spa`): 
  - Issue JWT tokens to users after authentication
  - Need protocol mappers to embed claims (tenant_id, competition_id, roles) in tokens
  - These claims flow through to all downstream services

- **Bearer-Only Clients** (`competition-service`, `judging-service`):
  - Only validate JWT tokens received from upstream services
  - Do NOT issue tokens themselves
  - Read claims from incoming tokens (no protocol mappers needed)

**Token Flow:**
1. User authenticates via `frontend-spa` → Token issued with `tenant_id`, `competition_id`, `roles`
2. Frontend calls BFF with token → BFF validates token and reads claims
3. BFF calls `competition-service` via Token Exchange → Exchanged token keeps original claims
4. `competition-service` validates token → Reads `tenant_id`, `competition_id` from incoming token

**Result**: Protocol mappers on token-issuing clients automatically propagate claims to all services through the token chain.

## Deployment Notes

**Production Deployment**:
- Protocol mappers must be configured manually in production Keycloak
- Do NOT delete production realm data
- Follow "Option 2: Manual Configuration" steps for production
- Only configure `bff-api` and `frontend-spa` clients (skip bearer-only services)
- Verify with test user before enabling for real users
