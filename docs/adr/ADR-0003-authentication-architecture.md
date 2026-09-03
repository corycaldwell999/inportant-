# ADR-0003: Authentication Architecture - JWT Tokens & Client-Side State Management

**Date**: 2026-09-02  
**Status**: Accepted  
**Context**: Phase 1c Web Application Authentication Implementation

## Decision

Use JWT (JSON Web Token) authentication with server-side refresh token validation and client-side context state management for the web application. Store auth tokens in localStorage with automatic refresh on app load.

## Rationale

### Why JWT Over Session-Based Auth?
1. **Stateless API**: Backend doesn't maintain session state; scales horizontally
2. **Mobile Friendly**: Works across multiple platforms without shared session storage
3. **Decoupled Clients**: Web, mobile, and future clients can auth independently
4. **Standards-Based**: OpenID Connect compatible for future OIDC integration

### Why Two-Token Strategy?
1. **Security**: Short-lived access tokens (1 hour) limit exposure of compromised tokens
2. **Refresh Isolation**: Refresh tokens kept secure, used only for token renewal
3. **Flexibility**: Can revoke refresh tokens server-side (via Session table)
4. **Best Practice**: Industry standard for OAuth 2.0 implementations

### Why Client-Side Context State?
1. **React Idiom**: Context API is standard React pattern (no external state library needed)
2. **Performance**: Reduces re-renders with focused context scope
3. **Simplicity**: Simpler than Redux for auth-only state management
4. **Dev Experience**: useAuth() hook is intuitive and minimal

### Why localStorage Over httpOnly Cookies?
**Context**: Phase 1 implements localStorage for dev/MVP; production will use httpOnly cookies

**localStorage (Current)**
- ✅ Easier initial development
- ✅ Works with modern dev tools
- ❌ Vulnerable to XSS attacks

**httpOnly Cookies (Production)**
- ✅ Not accessible to JavaScript (XSS protection)
- ✅ Automatically sent with requests
- ❌ Requires CSRF protection
- ❌ More complex backend handling

**Migration Path**: Backend already supports both; frontend will switch to cookie-based approach in Phase 2 security hardening.

## Implementation Details

### Token Structure
```typescript
interface AuthToken {
  accessToken: string;        // JWT, 1-hour expiry
  refreshToken?: string;      // Opaque token, 7-day expiry
  expiresIn: number;          // Seconds until expiry
  tokenType: 'Bearer';        // OAuth 2.0 scheme
}
```

### JWT Payload (HS256)
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "displayName": "User Name",
  "roles": ["user"],
  "iat": 1693651200,
  "exp": 1693654800,
  "iss": "trauma-platform-identity-api",
  "aud": "trauma-platform-clients"
}
```

### Context Flow
```
┌─────────────────────────────────────┐
│   AuthProvider (Root Layout)        │
├─────────────────────────────────────┤
│  • Load tokens from localStorage    │
│  • Initialize API client with token │
│  • Provide useAuth() hook           │
└──────────────┬──────────────────────┘
               │
       ┌───────▼────────┐
       │  useAuth Hook  │
       ├────────────────┤
       │ • user state   │
       │ • token state  │
       │ • login()      │
       │ • register()   │
       │ • logout()     │
       │ • refreshToken │
       └───────┬────────┘
               │
       ┌───────▼──────────────────┐
       │  Protected Components    │
       ├────────────────────────────┤
       │ useProtectedRoute() hook  │
       │ Redirects to /login if    │
       │ not authenticated         │
       └────────────────────────────┘
```

### Refresh Token Flow
```
1. User logs in → receive accessToken + refreshToken
2. accessToken stored in context + localStorage
3. API calls use: Authorization: Bearer {accessToken}
4. When accessToken nears expiry or API returns 401:
   - POST /api/v1/auth/refresh with refreshToken
   - Receive new accessToken + refreshToken
   - Update localStorage & context
   - Retry original request
5. If refreshToken invalid/expired:
   - Clear auth state
   - Redirect to /login
```

### Security Considerations

#### Implemented
- ✅ Passwords hashed with bcrypt (12-round work factor)
- ✅ Refresh tokens invalidated on logout
- ✅ Access tokens have short expiry (1 hour)
- ✅ Token validation on every API request
- ✅ Soft delete prevents data resurrection

#### Phase 2 Security Hardening
- [ ] Migrate from localStorage to httpOnly cookies
- [ ] Implement CSRF token validation
- [ ] Add rate limiting on auth endpoints (5 attempts/5 min)
- [ ] Implement account lockout after failed attempts
- [ ] Add email verification requirement
- [ ] Add optional 2FA/MFA
- [ ] Implement device fingerprinting
- [ ] Add session revocation for all devices
- [ ] Audit logging for all auth events

## Alternatives Considered

### Option 1: OAuth 2.0 / OpenID Connect
**Pros**: Industry standard, delegated auth, social login  
**Cons**: Complex infrastructure, requires identity provider  
**Decision**: Defer to Phase 3; JWT foundation enables OIDC bridge in future

### Option 2: Multi-Factor Authentication (MFA) from Start
**Pros**: Maximum security  
**Cons**: Complex UX, slows MVP development  
**Decision**: Database schema ready; implement in Phase 1d after core flows stable

### Option 3: Server-Side Session Store (Redis)
**Pros**: Revoke sessions immediately  
**Cons**: Stateful backend, more infrastructure  
**Decision**: Current Session table in PostgreSQL sufficient for Phase 1; can add Redis layer later

### Option 4: Separate Auth Service / Keycloak
**Pros**: Dedicated auth provider, industry-standard  
**Cons**: Additional infrastructure, operational complexity  
**Decision**: In-service auth sufficient for Phase 1; can extract later as microservice

## Consequences

### Positive
- ✅ Clean separation: API authentication vs web app state management
- ✅ Easy horizontal scaling of API (stateless)
- ✅ Works across web and mobile without changes
- ✅ Simple refresh token revocation mechanism
- ✅ Standards-aligned for future integrations

### Negative
- ❌ Token compromise affects auth until expiry (mitigation: short expiry)
- ❌ XSS vulnerability with localStorage (mitigation: migrate to httpOnly cookies)
- ❌ No built-in session listing/revocation per device (mitigation: Sessions table querying)
- ❌ Manual refresh token rotation (mitigation: automatic refresh endpoint)

### Maintenance Burden
- Token validation on every API request
- Refresh token expiry monitoring
- Session cleanup of revoked tokens (optional, can run as scheduled task)

## Verification

### Unit Tests
- [x] JWT token generation with correct claims
- [x] Token validation and expiry checking
- [x] Password hashing and verification
- [x] Refresh token rotation

### Integration Tests
- [x] Registration flow (validate → hash → store → return tokens)
- [x] Login flow (find user → verify password → generate tokens)
- [x] Refresh flow (validate refresh token → generate new pair)
- [x] Protected route access (valid token → allow, invalid → 401)

### Security Audit
- [x] Tokens contain no sensitive data
- [x] Password stored as hash only
- [x] Refresh tokens invalidated on logout
- [x] Database connection encrypted in flight

## Future Evolution

### Phase 2: Session Management
```csharp
// List user's active sessions
GET /api/v1/sessions

// Revoke specific device
DELETE /api/v1/sessions/{sessionId}

// Logout all devices
POST /api/v1/sessions/logout-all
```

### Phase 2: Email Verification
```
1. Registration creates user with emailVerified: false
2. Send verification email with link
3. Link contains one-time token
4. Clicking link verifies email
5. User can't access certain features until verified (optional policy)
```

### Phase 2: Password Reset
```
1. User requests password reset
2. Send email with one-time reset token
3. User enters new password with token
4. Invalidate existing refresh tokens
5. Require re-login
```

### Phase 3: Multi-Factor Authentication
```
1. User enables TOTP/SMS during settings
2. On login, after password verification, prompt for second factor
3. Generate session token (not full auth) pending MFA
4. Store MFA factor in JWT after verification
```

### Phase 3: OpenID Connect (OIDC)
```
Current JWT + refresh approach enables future OIDC bridge:
- We become OIDC provider (identity-api handles OIDC flow)
- Clients (web, mobile, 3rd party) authenticate via OIDC
- Backend validates OIDC tokens from identity-api
- Single source of truth for user identity
```

## References

- [RFC 7519 - JSON Web Tokens](https://tools.ietf.org/html/rfc7519)
- [RFC 6749 - OAuth 2.0 Authorization Framework](https://tools.ietf.org/html/rfc6749)
- [OWASP - Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [ASP.NET Core JWT Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [React Context API Documentation](https://react.dev/learn/passing-data-deeply-with-context)

---

**ADR Status**: Accepted by development team for Phase 1c implementation.  
**Implementation Date**: 2026-09-02  
**Last Reviewed**: 2026-09-02
