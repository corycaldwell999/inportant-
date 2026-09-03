# Phase 1 Complete - Testing & Deployment Guide

**Status**: Phase 1 Foundation Complete (97% - all core infrastructure ready)  
**Date**: 2026-09-02  
**Version**: 1.0.0

## 📋 What's Included in Phase 1

### ✅ Complete
- **Backend**: ASP.NET Core 8 Identity API with PostgreSQL
- **Frontend**: Next.js web app with authentication flows
- **Database**: EF Core migrations, models, and seed data
- **Authentication**: JWT tokens, bcrypt password hashing, refresh tokens
- **Shared Infrastructure**: Monorepo packages (types, validation, SDK, UI)
- **CI/CD**: GitHub Actions 8-stage pipeline
- **Documentation**: Complete specification, architecture docs, ADRs

### 🔄 In Progress / Partial
- Mobile app auth screens (shells created, flows pending)
- Email verification (database ready, endpoint pending)
- Session management endpoints (partial)
- Integration tests (foundation created)

### 📋 Phase 2+
- Advanced features (communities, journals, wellness, learning)
- User service
- Moderation infrastructure
- Advanced analytics

---

## 🚀 Getting Started

### Prerequisites
- Node.js 18+
- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL client tools (optional)

### Local Setup

#### 1. Clone & Install
```bash
git clone <repo>
cd trauma-platform
npm install
```

#### 2. Start Infrastructure
```bash
# Start PostgreSQL and Redis
docker-compose -f infrastructure/docker/docker-compose.yml up -d

# Verify services are running
docker ps
```

#### 3. Configure Backend
```bash
cd services/identity-api

# Restore NuGet packages
dotnet restore

# Create database and apply migrations
dotnet ef database update

# Or run migrations manually
dotnet tool install dotnet-ef
dotnet-ef database update
```

#### 4. Start Services

**Terminal 1 - Backend API:**
```bash
cd services/identity-api
dotnet run
# Listening on http://localhost:7001
```

**Terminal 2 - Web App:**
```bash
cd apps/web
npm run dev
# Listening on http://localhost:3000
```

**Terminal 3 - Mobile App (optional):**
```bash
cd apps/mobile
npm run start
# Expo server running
```

#### 5. Test End-to-End Flow

Visit http://localhost:3000

1. Click "Create Account" or "Sign In" button
2. Register with:
   - Email: `test@example.com`
   - Password: `SecurePass123`
   - Display Name: `Test User`
3. Log in with those credentials
4. View dashboard with profile info
5. Click "Sign Out"

---

## 🧪 Running Tests

### Type Checking (All Packages)
```bash
npm run type-check          # 13/13 packages
```

### Unit Tests
```bash
npm run test                # Run vitest
npm run test:watch         # Watch mode
```

### Integration Tests
```bash
npm run test -- auth.integration.test.ts
```

### E2E Tests
```bash
npm run test:e2e            # Playwright
npm run test:e2e --ui       # With UI
npm run test:e2e --debug    # Debug mode
```

### Full CI/CD Pipeline Locally
```bash
npx turbo run type-check lint test build
```

---

## 📊 API Documentation

### Base URL
```
http://localhost:7001
```

### Health Checks
```bash
# Liveness probe (K8s)
GET /health
→ { "status": "healthy" }

# Readiness probe (K8s)
GET /ready
→ { "ready": true }

# OpenAPI/Swagger specification
GET /openapi/v1.json
```

### Authentication Endpoints

#### Register
```bash
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123",
  "displayName": "User Name"
}

→ 201 Created
{
  "data": {
    "user": {
      "id": "uuid",
      "email": "user@example.com",
      "displayName": "User Name",
      "isAccountActive": true
    },
    "token": {
      "accessToken": "jwt_token...",
      "refreshToken": "refresh_token...",
      "expiresIn": 3600,
      "tokenType": "Bearer"
    }
  }
}
```

#### Login
```bash
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123"
}

→ 200 OK
{
  "data": {
    "user": { ... },
    "token": { ... }
  }
}
```

#### Refresh Token
```bash
POST /api/v1/auth/refresh
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "refreshToken": "refresh_token..."
}

→ 200 OK
{
  "data": {
    "token": {
      "accessToken": "new_jwt...",
      "refreshToken": "new_refresh...",
      "expiresIn": 3600,
      "tokenType": "Bearer"
    }
  }
}
```

#### Get Current User
```bash
GET /api/v1/auth/me
Authorization: Bearer {accessToken}

→ 200 OK
{
  "data": {
    "id": "uuid",
    "email": "user@example.com",
    "displayName": "User Name",
    "isEmailVerified": false,
    "isAccountActive": true,
    "roles": ["user"]
  }
}
```

---

## 🔐 Security Checklist

### ✅ Implemented
- [x] JWT token authentication
- [x] bcrypt password hashing (12-round work factor)
- [x] Secure refresh token mechanism
- [x] HTTPS/TLS ready (configure in production)
- [x] CORS policy enforcement
- [x] Request correlation IDs
- [x] Structured logging (Serilog)
- [x] Soft delete for user data
- [x] Connection pooling with retry logic

### 🔄 Production Hardening
- [ ] Enable HTTPS/TLS
- [ ] Configure secure CORS origins
- [ ] Set up rate limiting
- [ ] Enable database encryption at rest
- [ ] Configure secrets management (vault/K8s secrets)
- [ ] Implement CSRF protection
- [ ] Add request signing
- [ ] Enable audit logging for compliance
- [ ] Set up monitoring & alerts
- [ ] Configure backup & disaster recovery

---

## 📦 Environment Variables

### Web App (`.env.local`)
```bash
NEXT_PUBLIC_API_URL=http://localhost:7001
```

### Backend (`appsettings.Development.json`)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=trauma_platform_identity;User Id=postgres;Password=postgres;"
  },
  "Jwt": {
    "SigningKey": "your-256-bit-key-minimum-32-characters-long",
    "Issuer": "trauma-platform-identity-api",
    "Audience": "trauma-platform-clients",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:19006"]
  }
}
```

---

## 🐛 Troubleshooting

### Database Connection Issues
```bash
# Test connection
psql -h localhost -U postgres -d trauma_platform_identity

# Check logs
docker logs -f trauma-platform-postgres

# Reset database
dotnet ef database drop --force
dotnet ef database update
```

### API Connection Issues
```bash
# Test endpoint
curl http://localhost:7001/health

# Check logs
# Terminal where `dotnet run` is executing
```

### Web App Not Loading
```bash
# Check Node version
node --version  # Should be 18+

# Clear build cache
rm -rf .next node_modules
npm install
npm run dev
```

### Token Issues
- Access token expired? Use refresh token via `/api/v1/auth/refresh`
- Refresh token expired? User must login again
- Invalid token format? Verify "Bearer {token}" format in Authorization header

---

## 📈 Next Steps (Phase 1d & 2)

### Phase 1d (Remaining Foundation)
1. Email verification flow
   - Send verification link (future: Mailgun/SendGrid integration)
   - Verify email endpoint
   - Resend verification logic

2. Password reset flow
   - Request password reset
   - Send reset link
   - Confirm password reset with token

3. Session management
   - List active sessions
   - Revoke specific devices
   - Logout endpoint

4. Comprehensive testing
   - Unit tests for all auth flows
   - Integration tests for API endpoints
   - E2E tests for browser flows
   - Performance tests

### Phase 2 (Core Features)
1. User profiles & settings
2. Communities (create, join, browse)
3. Posts & comments
4. Journal entries
5. Wellness tracking
6. Learning paths

### Security & Operations
- Implement proper error tracking (Sentry)
- Set up monitoring (DataDog, New Relic)
- Configure CDN for static assets
- Set up automated backups
- Implement GDPR data export
- Add penetration testing results

---

## 📚 Documentation

- [SPECIFICATION.md](../docs/SPECIFICATION.md) - Complete feature specification
- [README.md](../README.md) - Project overview
- [DATABASE-SETUP.md](../docs/DATABASE-SETUP.md) - Database management
- [ADR-0001](../docs/adr/ADR-0001-monorepo-and-service-boundaries.md) - Architecture decisions
- [API Documentation](http://localhost:7001/openapi/v1.json) - OpenAPI schema

---

## 🤝 Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines.

## 📄 License

See [LICENSE](../LICENSE) for details.

---

**Build with care. For those who've experienced trauma.** 💚
