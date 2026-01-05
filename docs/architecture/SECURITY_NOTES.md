# Security Notes and Known Concerns

This document tracks security considerations and known concerns in the application that require architectural decisions or future improvements.

## 1. LocalStorage Security Concern (MVP Phase)

**Status**: Known Technical Debt  
**Severity**: Medium  
**Context**: Issue #90, PR #106 Review Comments  
**Date Identified**: 2025-01-05

### Current Implementation

The frontend currently stores sensitive authentication data in `localStorage`:
- `tenant_id`: Tenant identifier extracted from JWT
- `user_id`: User identifier for session tracking
- `auth_token`: JWT authentication token (future implementation)

**Location**: 
- `src/frontend/src/components/OrganizerRegistrationForm.tsx` (lines 28-29)
- `src/frontend/src/api/client.ts` (line 39)

### Security Implications

1. **XSS Vulnerability**: `localStorage` is accessible to any JavaScript code running on the page, making the application vulnerable to Cross-Site Scripting (XSS) attacks
2. **Session Hijacking**: If malicious scripts gain access to stored tokens, they can impersonate users
3. **Data Persistence**: `localStorage` data persists across browser sessions and is not cleared automatically

### Risk Mitigation (Current)

1. ✅ **CSP Headers**: Content Security Policy headers should be configured to prevent inline scripts
2. ✅ **HTTPS Only**: All traffic is encrypted in transit
3. ✅ **Token Expiration**: JWT tokens have short expiration times (configured in Keycloak)
4. ⚠️ **XSS Prevention**: Proper input sanitization and React's default XSS protection

### Recommended Future Solutions

**Option 1: HttpOnly Cookies (Recommended)**
```typescript
// Backend sets cookie on login/registration
res.cookie('auth_token', token, {
  httpOnly: true,  // Not accessible to JavaScript
  secure: true,    // HTTPS only
  sameSite: 'strict',
  maxAge: 3600000  // 1 hour
});

// Frontend doesn't need to store token
// Browser automatically sends cookie with requests
```

**Pros**: 
- Immune to XSS attacks
- Industry standard for sensitive tokens
- Automatic CSRF protection with `sameSite: 'strict'`

**Cons**:
- Requires backend changes
- More complex CSRF handling if `sameSite: 'lax'`

**Option 2: Memory-Only Storage with Refresh Tokens**
```typescript
// Store in React state/context (memory only)
const AuthContext = createContext();
// Lost on page refresh, but use refresh token flow
```

**Pros**:
- No persistence vulnerability
- Short-lived tokens in memory

**Cons**:
- Requires refresh token implementation
- Poor UX on page refresh without proper handling

**Option 3: Secure Token Service**
```typescript
// Use BFF (Backend-for-Frontend) pattern
// Frontend never sees actual tokens
// BFF handles all token management
```

**Pros**:
- Zero token exposure to frontend
- Best security posture

**Cons**:
- Significant architectural change
- Already using BFF for validation, but not for storage

### Action Items

- [ ] **Immediate (MVP)**: Document this as known technical debt
- [ ] **Post-MVP**: Implement HttpOnly cookies for authentication tokens
- [ ] **Post-MVP**: Move tenant_id and user_id to secure cookies or server-side session
- [ ] **Post-MVP**: Add ADR documenting the security architecture decision
- [ ] **Post-MVP**: Implement proper CSRF protection if using cookies

### References

- **OWASP**: [Session Management Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html)
- **OWASP**: [XSS Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- **ADR-004**: Authentication & Authorization (existing)
- **PR #106 Review**: Original identification of this concern

---

## 2. Future Security Considerations

### Planned Enhancements

1. **Rate Limiting**: Implement rate limiting on authentication endpoints
2. **Audit Logging**: Add comprehensive audit logging for sensitive operations
3. **Secrets Management**: Migrate to Azure Key Vault for production secrets
4. **Database Encryption**: Enable encryption at rest for sensitive PII data

### Security Testing Requirements

- [ ] OWASP ZAP scanning for vulnerabilities
- [ ] Penetration testing before production launch
- [ ] Regular dependency vulnerability scanning (Dependabot enabled)
- [ ] Security code reviews for authentication/authorization changes

---

**Last Updated**: 2025-01-05  
**Next Review Date**: Before Production Launch
