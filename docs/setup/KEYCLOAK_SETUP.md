# Keycloak Setup Guide

**Date**: 2025-12-19  
**Version**: Keycloak 23.0  
**Environment**: Development (Docker Compose)

---

## Overview

This guide explains how to set up Keycloak OIDC authentication for the Beer Competition SaaS Platform. Keycloak provides:
- **OpenID Connect (OIDC)** authentication
- **JWT tokens** with RS256 signing (HS256 in development)
- **Role-based access control** (RBAC)
- **Multi-tenant support** via `tenant_id` claim

---

## Prerequisites

- Docker Desktop installed
- PostgreSQL running (via docker-compose)
- Git repository cloned
- Environment variables configured in `.env` file

---

## Quick Start

### 1. Start Infrastructure Services

```bash
# Navigate to infrastructure directory
cd infrastructure

# Start all services (PostgreSQL, RabbitMQ, Redis, Keycloak)
docker-compose up -d

# Check Keycloak is healthy
docker-compose ps keycloak
```

**Expected output:**
```
NAME                  IMAGE                              STATUS        PORTS
beercomp_keycloak     quay.io/keycloak/keycloak:23.0    Up (healthy)  0.0.0.0:8080->8080/tcp
```

### 2. Access Keycloak Admin Console

1. Open browser: **http://localhost:8080**
2. Click **Administration Console**
3. Login with credentials from `.env`:
   - **Username**: `admin` (from `KEYCLOAK_ADMIN`)
   - **Password**: `<your-password>` (from `KEYCLOAK_ADMIN_PASSWORD`)

### 3. Verify Realm Import

The `beercomp` realm is automatically imported on first startup from:
```
infrastructure/keycloak/realm-export.json
```

**Check realm is active:**
1. Hover over **Keycloak** dropdown (top-left)
2. Select **beercomp** realm
3. You should see **Realm Settings** page

---

## Realm Configuration

### Realm: `beercomp`

**Basic Settings:**
- **Realm ID**: `beercomp`
- **Display Name**: Beer Competition Platform
- **Enabled**: Yes
- **SSL Required**: External Only (HTTP allowed for localhost development)
- **Access Token Lifespan**: 15 minutes (900 seconds)
- **Refresh Token Max**: 24 hours

### Roles

Four realm roles are defined:

| Role       | Description                                                 |
|------------|-------------------------------------------------------------|
| `organizer`| Competition organizer - creates competitions, manages entries|
| `judge`    | BJCP certified judge - views flights, submits scoresheets  |
| `entrant`  | Competition entrant - registers beers, pays fees, views results|
| `steward`  | Competition steward - checks in bottles, manages logistics |

**View roles:**
- Navigate to **Realm Settings** → **Roles** → **Realm Roles**
- All 4 roles should be listed

### Clients

Four clients are configured for service-specific audiences and token exchange:

#### 1. `bff-api` (Confidential Client - API Gateway)

**Purpose**: BFF/API Gateway - validates frontend tokens and performs token exchange

**Configuration:**
- **Client ID**: `bff-api`
- **Client Protocol**: `openid-connect`
- **Access Type**: Confidential
- **Standard Flow**: Enabled
- **Direct Access Grants**: Enabled (for testing with Postman)
- **Service Accounts**: Enabled
- **Token Exchange**: Enabled (`oauth2.token.exchange.grant.enabled`)
- **Valid Redirect URIs**:
  - `http://localhost:5190/*`
  - `https://localhost:7038/*`
- **Web Origins**: 
  - `http://localhost:5190`
  - `https://localhost:7038`
  - `http://localhost:5173`
  - `https://localhost:5173`

**Client Secret:**
- Navigate to **Clients** → **bff-api** → **Credentials** tab
- Copy the **Client Secret** (or use placeholder in realm JSON)
- Store in `.env` file: `KEYCLOAK_BFF_CLIENT_SECRET=<secret>`

**Audience Claim:**
- Token includes `aud: "bff-api"` claim via audience mapper

#### 2. `competition-service` (Bearer-Only Client)

**Purpose**: Competition microservice - validates service-specific tokens from BFF

**Configuration:**
- **Client ID**: `competition-service`
- **Client Protocol**: `openid-connect`
- **Access Type**: Bearer-Only
- **Service Accounts**: Enabled
- **Valid Redirect URIs**: None (bearer-only)

**Client Secret:**
- Store in `.env` file: `KEYCLOAK_COMPETITION_CLIENT_SECRET=<secret>`

**Audience Claim:**
- Token includes `aud: "competition-service"` claim via audience mapper

#### 3. `judging-service` (Bearer-Only Client)

**Purpose**: Judging microservice - validates service-specific tokens from BFF

**Configuration:**
- **Client ID**: `judging-service`
- **Client Protocol**: `openid-connect`
- **Access Type**: Bearer-Only
- **Service Accounts**: Enabled
- **Valid Redirect URIs**: None (bearer-only)

**Client Secret:**
- Store in `.env` file: `KEYCLOAK_JUDGING_CLIENT_SECRET=<secret>`

**Audience Claim:**
- Token includes `aud: "judging-service"` claim via audience mapper

#### 4. `frontend-spa` (Public Client with PKCE)

**Purpose**: React SPA authentication

**Configuration:**
- **Client ID**: `frontend-spa`
- **Client Protocol**: `openid-connect`
- **Access Type**: Public
- **Standard Flow**: Enabled (Authorization Code + PKCE)
- **Implicit Flow**: Disabled (deprecated)
- **Direct Access Grants**: Disabled (not secure for SPAs)
- **Valid Redirect URIs**:
  - `http://localhost:5173/*`
  - `http://localhost:5173/auth/callback`
- **Web Origins**: 
  - `http://localhost:5173`
- **PKCE Code Challenge Method**: S256

**Audience Claim:**
- Token includes `aud: "bff-api"` claim via audience mapper

---

## Token Exchange Flow (OAuth 2.0 RFC 8693)

The BFF implements **OAuth 2.0 Token Exchange** to convert frontend tokens into service-specific tokens:

### Flow Overview

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
       │    audience=competition-service
       │
       ▼
┌─────────────┐
│  Keycloak   │  5. Return JWT with aud: "competition-service"
│             │  ────────────────────────────────────────────►
└─────────────┘
       │
       │ 6. Forward request with service token
       ▼
┌──────────────────┐
│  Competition     │  7. Validate token (audience check)
│  Service         │
└──────────────────┘
```

### Benefits

1. **Audience Isolation**: Each service validates tokens with its specific audience
2. **Zero Trust**: Services cannot accept tokens meant for other services
3. **Token Scoping**: BFF can request minimal scopes for each service
4. **Security**: Prevents token reuse across service boundaries

### Configuration

**BFF `appsettings.json`:**
```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/beercomp",
    "Audience": "bff-api",
    "ClientId": "bff-api",
    "ClientSecret": "<secret>",
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

---

## Protocol Mappers

All clients have custom protocol mappers to include required claims in JWT tokens:

### 1. `audience` Mapper

**Type**: Audience  
**Included Client Audience**: `<client-id>` (e.g., `bff-api`, `competition-service`)  
**Add to ID Token**: No  
**Add to Access Token**: Yes

### 2. `tenant_id` Mapper

**Type**: User Attribute  
**User Attribute**: `tenant_id`  
**Token Claim Name**: `tenant_id`  
**Claim JSON Type**: String  
**Add to ID Token**: Yes  
**Add to Access Token**: Yes  
**Add to Userinfo**: Yes

### 3. `roles` Mapper

**Type**: User Realm Role  
**Token Claim Name**: `roles`  
**Claim JSON Type**: String (multivalued)  
**Add to ID Token**: Yes  
**Add to Access Token**: Yes  
**Add to Userinfo**: Yes

---

## Test Users

Four test users are pre-configured in the realm:

| Username                  | Password      | Roles              | Tenant ID                            |
|---------------------------|---------------|--------------------|--------------------------------------|
| `organizer@beercomp.local`| `organizer123`| `organizer`        | `11111111-1111-1111-1111-111111111111` |
| `judge@beercomp.local`    | `judge123`    | `judge`, `entrant` | `11111111-1111-1111-1111-111111111111` |
| `entrant@beercomp.local`  | `entrant123`  | `entrant`          | `11111111-1111-1111-1111-111111111111` |
| `steward@beercomp.local`  | `steward123`  | `steward`          | `11111111-1111-1111-1111-111111111111` |

**View users:**
- Navigate to **Users** → **View all users**
- Click on a user to see roles and attributes

---

## Testing Authentication

### Option 1: Using Postman

#### Step 1: Get Access Token (Direct Grant)

**Request:**
```http
POST http://localhost:8080/realms/beercomp/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&client_id=bff-api
&client_secret=<your-client-secret>
&username=organizer@beercomp.local
&password=organizer123
&scope=openid profile email roles
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires_in": 900,
  "refresh_expires_in": 86400,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "not-before-policy": 0,
  "session_state": "uuid",
  "scope": "openid profile email roles"
}
```

#### Step 2: Call Protected API

**Request:**
```http
GET http://localhost:5190/api/competitions
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response:**
```json
[
  {
    "id": "uuid",
    "name": "Spring Classic 2025",
    "registrationDeadline": "2025-03-15T00:00:00Z"
  }
]
```

### Option 2: Using cURL

```bash
# Get access token
TOKEN=$(curl -s -X POST \
  http://localhost:8080/realms/beercomp/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=bff-api" \
  -d "client_secret=<your-secret>" \
  -d "username=organizer@beercomp.local" \
  -d "password=organizer123" \
  -d "scope=openid profile email roles" \
  | jq -r '.access_token')

# Call protected API
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5190/api/competitions
```

### Option 3: Decode JWT Token

Use [jwt.io](https://jwt.io) to decode the access token and verify claims:

**Expected claims:**
```json
{
  "exp": 1703001234,
  "iat": 1703000334,
  "jti": "uuid",
  "iss": "http://localhost:8080/realms/beercomp",
  "aud": "bff-api",
  "sub": "user-uuid",
  "typ": "Bearer",
  "azp": "bff-api",
  "session_state": "uuid",
  "scope": "openid profile email roles",
  "email_verified": true,
  "preferred_username": "organizer",
  "email": "organizer@beercomp.local",
  "tenant_id": "11111111-1111-1111-1111-111111111111",
  "roles": ["organizer"]
}
```

---

## Troubleshooting

### Issue: "Invalid issuer" error

**Symptom**: BFF logs show `IDX10205: Issuer validation failed`

**Solution**:
1. Check `KC_HOSTNAME_URL` in docker-compose.yml matches `Keycloak:Authority` in appsettings.json
2. Verify `http://localhost:8080/realms/beercomp/.well-known/openid-configuration` is accessible
3. Restart Keycloak: `docker-compose restart keycloak`

### Issue: Realm not imported

**Symptom**: Logging in shows "Realm not found"

**Solution**:
1. Check volume mount: `./keycloak/realm-export.json:/opt/keycloak/data/import/realm-export.json:ro`
2. Verify file exists: `ls infrastructure/keycloak/realm-export.json`
3. Check Keycloak logs: `docker-compose logs keycloak | grep import`
4. Recreate container: `docker-compose down keycloak && docker-compose up -d keycloak`

### Issue: Missing `tenant_id` claim

**Symptom**: BFF returns `403 Forbidden` with "Missing tenant_id in authentication token"

**Solution**:
1. Verify user has `tenant_id` attribute:
   - Keycloak Admin → **Users** → Select user → **Attributes** tab
   - Add attribute: `tenant_id` = `11111111-1111-1111-1111-111111111111`
2. Verify protocol mapper is configured (see "Protocol Mappers" section)
3. Get a new access token (old tokens won't have the claim)

### Issue: Roles not in token

**Symptom**: Authorization fails even with correct user role

**Solution**:
1. Verify user has assigned roles:
   - Keycloak Admin → **Users** → Select user → **Role Mappings** tab
   - Assign required realm roles
2. Check protocol mapper "roles" is configured correctly
3. Verify `RoleClaimType = "roles"` in AuthenticationExtensions.cs
4. Get a new access token

---

## Production Considerations

⚠️ **IMPORTANT**: The following settings are for **development only**:

| Setting                     | Development Value       | Production Value                        |
|-----------------------------|-------------------------|-----------------------------------------|
| `KC_HTTP_ENABLED`          | `true`                  | `false` (HTTPS only)                    |
| `KC_HOSTNAME_STRICT`       | `false`                 | `true`                                  |
| `RequireHttpsMetadata`     | `false`                 | `true`                                  |
| `KEYCLOAK_ADMIN_PASSWORD`  | In `.env` file          | Azure Key Vault / AWS Secrets Manager   |
| Client Secrets             | In realm JSON           | Azure Key Vault / AWS Secrets Manager   |
| Token Signing              | HS256 (symmetric)       | RS256 (asymmetric)                      |

**Production checklist:**
- [ ] Enable HTTPS with valid SSL certificates
- [ ] Use Azure Key Vault for secrets
- [ ] Configure Keycloak clustering for high availability
- [ ] Set up database backups (Keycloak realm data in PostgreSQL)
- [ ] Enable Multi-Factor Authentication (MFA) for organizers
- [ ] Configure rate limiting on token endpoint
- [ ] Set up monitoring and alerting (failed login attempts, token expiry)
- [ ] Review CORS configuration (restrict to production frontend domain)

---

## Next Steps

1. **Frontend Integration**: Follow [frontend/docs/AUTHENTICATION.md](../../frontend/docs/AUTHENTICATION.md) to integrate Keycloak with React SPA
2. **Role-Based UI**: Implement conditional rendering based on user roles
3. **Token Refresh**: Implement automatic token refresh before expiry
4. **Logout Flow**: Implement proper logout with token revocation
5. **User Profile**: Create user profile page to display email, roles, tenant

---

## References

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [OpenID Connect Specification](https://openid.net/connect/)
- [ADR-004: Authentication & Authorization](../architecture/decisions/ADR-004-authentication-authorization.md)
- [JWT.io](https://jwt.io) - JWT token decoder
- [PKCE RFC 7636](https://datatracker.ietf.org/doc/html/rfc7636) - Proof Key for Code Exchange

---

**Questions?** Check the [Project README](../../README.md) or contact the backend team.
