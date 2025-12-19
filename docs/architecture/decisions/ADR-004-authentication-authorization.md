# ADR-004: Authentication & Authorization

**Date**: 2025-12-19  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

Our platform requires secure authentication and fine-grained authorization for multiple user roles:
- **Organizers**: Create competitions, manage entries, publish results
- **Judges**: View flight assignments, submit scoresheets, participate in Best of Show
- **Entrants**: Register beers, pay fees, view results
- **Stewards**: Check in bottles, manage physical logistics

Requirements:
- **Multi-tenancy**: Users isolated to their organization's competitions
- **SSO Support**: Enable enterprise customers to use existing identity providers (Google, Azure AD)
- **Token-based**: Support stateless API authentication for React SPA
- **Role-based Access Control (RBAC)**: Enforce permissions at API gateway and microservice levels
- **Audit Trail**: Track all authentication events for compliance

How do we implement authentication/authorization that balances **security, developer experience, and operational simplicity**?

---

## Decision Drivers

- **Security**: Industry-standard protocols (OIDC/OAuth2), no homegrown auth
- **Multi-tenancy**: Tenant isolation enforced in JWT tokens
- **SSO Support**: OIDC integration with Google, Azure AD, Okta
- **Token-based**: Stateless authentication for SPA and mobile apps
- **Developer Experience**: Simple integration for microservices
- **Centralized Management**: Single source of truth for users/roles
- **Compliance**: Audit logs for GDPR, SOC 2
- **Cost**: Open-source first, managed services for scalability

---

## Considered Options

### 1. Homegrown JWT Authentication
**Approach**: Build custom auth service with JWT generation

**Pros:**
- ✅ Full control over implementation
- ✅ No external dependencies

**Cons:**
- ❌ **Security risk**: Easy to introduce vulnerabilities
- ❌ **No SSO**: Must implement OIDC/SAML integrations manually
- ❌ **Maintenance burden**: Token rotation, password policies, MFA
- ❌ **Compliance**: Must implement audit logging ourselves

---

### 2. Auth0 (Managed Service)
**Approach**: SaaS identity provider with OIDC support

**Pros:**
- ✅ Turnkey solution with SSO integrations
- ✅ Excellent developer experience
- ✅ Built-in MFA, passwordless authentication

**Cons:**
- ❌ **Cost**: Expensive at scale ($800+/month for 10,000 users)
- ❌ **Vendor lock-in**: Migration difficult
- ❌ **Data residency**: User data stored on Auth0 servers

---

### 3. Keycloak (Open Source)
**Approach**: Self-hosted OIDC/OAuth2 server

**Pros:**
- ✅ **Open-source**: No licensing costs
- ✅ **OIDC/OAuth2 compliant**: Industry-standard protocols
- ✅ **SSO integrations**: Google, Azure AD, GitHub, SAML
- ✅ **Multi-tenancy**: Realm-per-tenant or custom user federation
- ✅ **Self-hosted**: Full control over data
- ✅ **Mature**: Battle-tested by Red Hat/CNCF

**Cons:**
- ❌ **Operational complexity**: Requires database, clustering for HA
- ❌ **Learning curve**: Admin UI complexity

---

### 4. Azure AD B2C (Managed Service)
**Approach**: Microsoft's identity service for customer-facing apps

**Pros:**
- ✅ Azure-native (seamless integration)
- ✅ SSO support
- ✅ Managed service (no ops burden)

**Cons:**
- ❌ **Vendor lock-in**: Tight Azure coupling
- ❌ **Cost**: Expensive for high MAU (Monthly Active Users)
- ❌ **Limited customization**: Branded login pages require premium tier

---

## Decision Outcome

**Chosen Option**: **#3 - Keycloak + BFF (Backend-for-Frontend) Pattern**

---

## Architecture Overview

```
┌─────────────┐
│  React SPA  │
│   (PWA)     │
└──────┬──────┘
       │ 1. Login Request
       ▼
┌─────────────────┐
│  BFF/API Gateway│  2. Redirect to Keycloak
│   (.NET 10)     │  ◄───────────────────────┐
└──────┬──────────┘                          │
       │ 3. Authorization Code               │
       ▼                                     │
┌──────────────────┐                         │
│    Keycloak      │  4. Token Exchange      │
│  (OIDC Provider) │  ────────────────────►  │
└──────────────────┘                         │
                                             │
       ┌─────────────────────────────────────┘
       │ 5. JWT (access + refresh tokens)
       ▼
┌─────────────────┐
│  BFF Validates  │  6. Proxy to Microservices
│  JWT & Enforces │  ───────────────────────►
│  Role Policies  │
└─────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│  Microservices (Competition,     │
│  Judging) trust BFF and extract  │
│  tenant_id + user claims from    │
│  X-Tenant-ID header               │
└──────────────────────────────────┘
```

---

## Implementation Details

### 1. Keycloak Configuration

#### Realm Setup
```bash
# Create realm: beer-competition-saas
Realm: beer-competition-saas
Themes: Custom (branded login page)
Session Settings:
  - Access Token Lifespan: 15 minutes
  - Refresh Token Lifespan: 30 days
  - SSO Session Idle: 12 hours
```

#### Client Configuration
```json
{
  "clientId": "beer-competition-spa",
  "protocol": "openid-connect",
  "publicClient": false,
  "redirectUris": [
    "https://app.beercompetition.com/auth/callback",
    "http://localhost:3000/auth/callback"
  ],
  "webOrigins": ["+"],
  "standardFlowEnabled": true,  // Authorization Code Flow
  "implicitFlowEnabled": false,  // Deprecated, don't use
  "directAccessGrantsEnabled": false,  // No password grants
  "serviceAccountsEnabled": false
}
```

#### Roles
```json
{
  "roles": [
    {"name": "organizer", "description": "Competition organizer"},
    {"name": "judge", "description": "BJCP-certified judge"},
    {"name": "entrant", "description": "Beer entrant"},
    {"name": "steward", "description": "Bottle steward"}
  ]
}
```

#### Custom User Attributes
```json
{
  "attributes": {
    "tenant_id": "123e4567-e89b-12d3-a456-426614174000",
    "bjcp_rank": "Certified",  // For judges
    "bjcp_id": "A1234"  // BJCP membership number
  }
}
```

#### Identity Providers (SSO)
```bash
# Google
Identity Provider: Google
Client ID: <from Google Console>
Client Secret: <from Google Console>
Scopes: openid email profile

# Azure AD
Identity Provider: Microsoft
Client ID: <from Azure AD>
Tenant ID: <organization tenant>
```

---

### 2. BFF (API Gateway) - Token Validation

#### Startup Configuration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Keycloak OIDC authentication
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = "https://keycloak.beercompetition.com/realms/beer-competition-saas";
            options.Audience = "beer-competition-spa";
            options.RequireHttpsMetadata = true;  // Enforce HTTPS in prod
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        });

    // Authorization policies
    services.AddAuthorization(options =>
    {
        options.AddPolicy("OrganizerOnly", policy => 
            policy.RequireRole("organizer"));
        
        options.AddPolicy("JudgeOnly", policy => 
            policy.RequireRole("judge"));
        
        options.AddPolicy("OrganizerOrJudge", policy => 
            policy.RequireRole("organizer", "judge"));
    });

    // Tenant extraction
    services.AddScoped<ITenantProvider, TenantProvider>();
}
```

#### Tenant Extraction Middleware
```csharp
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract tenant_id from JWT claims
        var tenantClaim = context.User.Claims
            .FirstOrDefault(c => c.Type == "tenant_id");

        if (tenantClaim == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "unauthorized", 
                message = "Missing tenant_id claim in token" 
            });
            return;
        }

        // Validate tenant exists
        if (!await _tenantRepository.TenantExistsAsync(Guid.Parse(tenantClaim.Value)))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "forbidden", 
                message = "Invalid tenant" 
            });
            return;
        }

        // Add tenant header for downstream services
        context.Request.Headers["X-Tenant-ID"] = tenantClaim.Value;

        await _next(context);
    }
}
```

#### Controller with Authorization
```csharp
[ApiController]
[Route("api/competitions")]
[Authorize]  // Requires authenticated user
public class CompetitionsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "OrganizerOnly")]  // Role-based authorization
    public async Task<IActionResult> CreateCompetition(
        [FromBody] CreateCompetitionRequest request)
    {
        // User.Claims automatically populated by JWT middleware
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = User.FindFirstValue("tenant_id");
        
        var result = await _mediator.Send(new CreateCompetitionCommand(
            TenantId: Guid.Parse(tenantId),
            UserId: Guid.Parse(userId),
            Name: request.Name,
            RegistrationDeadline: request.RegistrationDeadline
        ));

        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "OrganizerOrJudge")]  // Multiple roles allowed
    public async Task<IActionResult> GetCompetition(Guid id)
    {
        // ...
    }
}
```

---

### 3. JWT Token Structure

#### Access Token (JWT)
```json
{
  "sub": "user-uuid-123",
  "email": "organizer@example.com",
  "preferred_username": "john.doe",
  "tenant_id": "tenant-uuid-456",
  "roles": ["organizer", "judge"],
  "bjcp_rank": "Certified",
  "iss": "https://keycloak.beercompetition.com/realms/beer-competition-saas",
  "aud": "beer-competition-spa",
  "iat": 1734612000,
  "exp": 1734615600,  // 15-minute expiry
  "azp": "beer-competition-spa"
}
```

#### Refresh Token (Opaque)
- Stored securely in HTTP-only cookie (not accessible to JavaScript)
- Used to obtain new access tokens without re-authentication
- 30-day lifetime

---

### 4. React SPA - Authentication Flow

#### Login Button
```typescript
// src/auth/LoginButton.tsx
export const LoginButton = () => {
  const handleLogin = () => {
    // Redirect to Keycloak login page
    window.location.href = 
      'https://keycloak.beercompetition.com/realms/beer-competition-saas/protocol/openid-connect/auth' +
      '?client_id=beer-competition-spa' +
      '&redirect_uri=https://app.beercompetition.com/auth/callback' +
      '&response_type=code' +
      '&scope=openid email profile';
  };

  return <button onClick={handleLogin}>Login with Keycloak</button>;
};
```

#### Callback Handler
```typescript
// src/auth/AuthCallback.tsx
export const AuthCallback = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const code = params.get('code');

    if (code) {
      // Exchange authorization code for tokens (via BFF)
      fetch('/api/auth/callback', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ code })
      })
        .then(res => res.json())
        .then(data => {
          // Store access token (httpOnly cookie handled by BFF)
          localStorage.setItem('access_token', data.access_token);
          navigate('/dashboard');
        });
    }
  }, []);

  return <div>Authenticating...</div>;
};
```

#### Protected API Calls
```typescript
// src/api/client.ts
export const apiClient = {
  async get<T>(url: string): Promise<T> {
    const token = localStorage.getItem('access_token');
    
    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (response.status === 401) {
      // Token expired, redirect to login
      window.location.href = '/login';
      throw new Error('Unauthorized');
    }

    return response.json();
  }
};
```

---

### 5. Microservice Authorization

Microservices **trust the BFF** and extract user context from headers:

```csharp
// Competition Service - No JWT validation (BFF already validated)
public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid CurrentTenantId
    {
        get
        {
            var tenantHeader = _httpContextAccessor.HttpContext?
                .Request.Headers["X-Tenant-ID"].FirstOrDefault();
            
            return tenantHeader != null 
                ? Guid.Parse(tenantHeader) 
                : throw new UnauthorizedAccessException("No tenant context");
        }
    }
}
```

**Security Note**: Microservices should only be accessible via the BFF (not exposed publicly). Use network policies or API Management to enforce this.

---

## Authentication Flows

### Flow 1: Initial Login (Authorization Code Flow)
1. User clicks "Login" → SPA redirects to Keycloak
2. User authenticates (username/password or SSO)
3. Keycloak redirects back to SPA with **authorization code**
4. SPA sends code to BFF `/auth/callback`
5. BFF exchanges code for **access + refresh tokens** with Keycloak
6. BFF stores refresh token in HTTP-only cookie
7. BFF returns access token to SPA (stored in localStorage or memory)

### Flow 2: API Request with Valid Token
1. SPA includes `Authorization: Bearer <token>` header
2. BFF validates JWT signature, expiry, audience
3. BFF extracts `tenant_id` and `roles` from claims
4. BFF checks authorization policy (e.g., "OrganizerOnly")
5. BFF proxies request to microservice with `X-Tenant-ID` header
6. Microservice processes request with tenant context

### Flow 3: Token Refresh
1. Access token expires (15 minutes)
2. SPA detects 401 response
3. SPA calls BFF `/auth/refresh` (HTTP-only cookie sent automatically)
4. BFF uses refresh token to get new access token from Keycloak
5. BFF returns new access token to SPA
6. SPA retries original request with new token

### Flow 4: Logout
1. User clicks "Logout"
2. SPA calls BFF `/auth/logout`
3. BFF revokes refresh token with Keycloak
4. BFF clears HTTP-only cookie
5. SPA clears localStorage and redirects to login

---

## Consequences

### Positive
✅ **Industry Standard**: OIDC/OAuth2 best practices  
✅ **SSO Support**: Easy integration with Google, Azure AD, etc.  
✅ **Multi-tenancy**: Tenant isolation enforced in JWT claims  
✅ **Open Source**: No licensing costs, full control  
✅ **BFF Pattern**: Microservices don't need JWT validation logic  
✅ **Token Refresh**: Seamless UX (no forced re-login)  
✅ **Audit Trail**: Keycloak logs all authentication events  

### Negative
❌ **Operational Complexity**: Keycloak requires PostgreSQL, clustering for HA  
❌ **Learning Curve**: Admin UI complexity for team  
❌ **Token Storage**: localStorage vulnerable to XSS (mitigated with short expiry + refresh tokens in httpOnly cookies)  
❌ **Single Point of Failure**: Keycloak downtime blocks all logins  

### Risks
⚠️ **Keycloak Availability**: Requires HA setup (clustered instances + load balancer)  
⚠️ **JWT Expiry**: Short 15-minute expiry may annoy users (mitigated with refresh flow)  
⚠️ **CORS Configuration**: Must whitelist SPA origin in Keycloak  
⚠️ **Token Revocation**: Access tokens valid until expiry (can't revoke immediately)  

---

## Alternatives Considered

### Why not Auth0?
- ✅ **Keycloak open-source** avoids vendor lock-in
- ✅ **Cost savings**: $0 vs $800+/month at scale
- ✅ **Data control**: Self-hosted, GDPR-compliant

### Why not Azure AD B2C?
- ✅ **Portability**: Keycloak not tied to Azure
- ✅ **Cost**: Azure AD B2C expensive for high MAU
- ✅ **Customization**: Keycloak more flexible

### Why not homegrown auth?
- ❌ **Security risk**: Too easy to introduce vulnerabilities
- ❌ **Feature gap**: No SSO, MFA, audit logs out-of-the-box
- ❌ **Maintenance burden**: Focus on business logic, not auth infrastructure

---

## Related Decisions

- [ADR-002: Multi-Tenancy Strategy](ADR-002-multi-tenancy-strategy.md) (Tenant ID in JWT)
- [ADR-007: Frontend Architecture](ADR-007-frontend-architecture.md) (React SPA auth flow)

---

## References

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [OIDC Authorization Code Flow](https://openid.net/specs/openid-connect-core-1_0.html#CodeFlowAuth)
- [OAuth 2.0 Best Practices](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics)
- [BFF Pattern (Token Handler)](https://curity.io/resources/learn/the-token-handler-pattern/)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)
