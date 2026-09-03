# Quick Start Guide - Trauma-Informed Platform

## Prerequisites

- **Node.js** 18+ (https://nodejs.org)
- **npm** 10+ or **pnpm** 8+
- **Docker** & **Docker Compose** (https://docker.com)
- **.NET 8 SDK** (https://dotnet.microsoft.com/download)
- **Git** for version control

## Installation (5 minutes)

```bash
# 1. Clone the repository
git clone <repository-url>
cd trauma-platform

# 2. Install dependencies
npm install
# OR
pnpm install

# 3. Start Docker services (PostgreSQL + Redis)
npm run docker:up

# 4. Copy environment template
cp .env.example .env.local

# 5. Apply database migrations (Phase 1b)
# npm run db:migrate
```

## Development Setup

### Start All Services

```bash
# Terminal 1: Start web app (localhost:3000)
npm run dev --filter=@trauma-platform/web

# Terminal 2: Start mobile app (Expo)
npm run dev --filter=@trauma-platform/mobile

# Terminal 3: Start Identity API (localhost:7001)
npm run dev --filter=@trauma-platform/identity-api

# Terminal 4: Docker logs (optional)
npm run docker:logs
```

### Code Quality

```bash
# Lint code
npm run lint

# Format code
npm run format

# Type checking
npm run type-check

# Run tests
npm run test
```

## Project Structure

```
├── apps/
│   ├── web/          Next.js web application (React)
│   ├── mobile/       React Native mobile app (Expo)
│   ├── admin/        (Phase 2) Admin dashboard
│   └── moderator/    (Phase 2) Moderation interface
├── services/
│   ├── identity-api/ ASP.NET Core 8 authentication
│   ├── user-api/     (Phase 2) User profiles
│   └── community-api/ (Phase 2) Communities & posts
├── packages/
│   ├── types/        Shared TypeScript types (200+)
│   ├── validation/   Zod schemas for validation
│   ├── sdk/          Typed API client
│   ├── ui/           Design system components
│   ├── security/     Client-side security utilities
│   ├── config/       Shared configs (ESLint, Prettier, TS)
│   └── localization/ Internationalization setup
└── docs/
    ├── SPECIFICATION.md      Complete requirements
    ├── PHASE-1-SUMMARY.md    Implementation overview
    ├── README.md             Setup & commands
    └── adr/                  Architecture decisions
```

## Key Commands

```bash
# Development
npm run dev              # Start all services concurrently
npm run dev --filter=@trauma-platform/web  # Start specific app

# Building
npm run build            # Build all packages
npm run build --filter=@trauma-platform/web # Build specific

# Testing
npm run test             # Run all tests
npm run test --filter=@trauma-platform/web  # Test specific
npm run test:e2e         # Run E2E tests (web)

# Code Quality
npm run lint             # ESLint across all packages
npm run format           # Prettier formatting
npm run type-check       # TypeScript type checking

# Database
npm run docker:up        # Start PostgreSQL + Redis
npm run docker:down      # Stop services
npm run docker:logs      # View service logs
npm run docker:clean     # Remove volumes (data loss)
npm run db:migrate       # Run database migrations (Phase 1b)
npm run db:seed          # Seed initial data (Phase 1b)

# Docker Image Building
npm run docker:build     # Build API Docker images
npm run docker:push      # Push to registry

# Clean
npm run clean            # Remove all build artifacts
```

## API Endpoints (Phase 1)

**Base URL**: `http://localhost:7001`

**Health & Status:**
```
GET  /health           → Kubernetes liveness
GET  /ready            → Kubernetes readiness
GET  /live             → Live check
GET  /api/v1/system/info → Service metadata
GET  /openapi/v1.json  → OpenAPI specification
```

**Authentication (Foundation - Full implementation in Phase 1b):**
```
POST /api/v1/auth/register  → Register new user
POST /api/v1/auth/login     → Authenticate user
GET  /api/v1/auth/me        → Current user (requires Bearer token)
POST /api/v1/auth/refresh   → Refresh access token
```

**Headers:**
```
Content-Type: application/json
x-correlation-id: <uuid>  # Automatically included in responses
Authorization: Bearer <jwt_token>  # For protected endpoints
```

## Using the SDK

```typescript
import { createClient } from '@trauma-platform/sdk';

// Initialize client
const client = createClient({
  baseUrl: 'http://localhost:7001',
  apiVersion: 'v1'
});

// Authenticate
const response = await client.login({
  email: 'user@example.com',
  password: 'SecurePassword123'
});

// Use authenticated methods
const user = await client.getCurrentUser();
const communities = await client.listCommunities({ limit: 20 });
```

## Environment Variables

**Backend (.env.local):**
```
# Database
DATABASE_URL=postgresql://user:password@localhost:5432/trauma_platform
REDIS_URL=redis://localhost:6379

# JWT
JWT_SIGNING_KEY=your-32-character-minimum-signing-key
JWT_ISSUER=https://localhost:7001
JWT_AUDIENCE=trauma-platform
JWT_EXPIRATION_MINUTES=60

# CORS
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:3001

# Environment
ASPNETCORE_ENVIRONMENT=Development
NODE_ENV=development
```

## Testing

```bash
# Unit Tests (Frontend)
npm run test --filter=@trauma-platform/web

# Unit Tests (Backend)
npm run test --filter=@trauma-platform/identity-api

# E2E Tests (Web)
npm run test:e2e --filter=@trauma-platform/web

# All Tests with Coverage
npm run test -- --coverage
```

## Troubleshooting

### Docker Issues
```bash
# PostgreSQL connection refused?
npm run docker:down
npm run docker:up
docker-compose logs -f  # View detailed logs

# Reset database (WARNING: Data loss)
npm run docker:clean
npm run docker:up
npm run db:migrate
```

### Port Already in Use
```bash
# Kill process on port 3000
lsof -ti:3000 | xargs kill -9

# Kill process on port 7001
lsof -ti:7001 | xargs kill -9

# Kill process on port 5432 (PostgreSQL)
lsof -ti:5432 | xargs kill -9
```

### TypeScript Errors
```bash
# Clean and rebuild
npm run clean
npm install
npm run type-check
```

## Documentation

- **Full Specification**: [SPECIFICATION.md](SPECIFICATION.md)
- **Phase 1 Summary**: [PHASE-1-SUMMARY.md](PHASE-1-SUMMARY.md)
- **Architecture Overview**: [docs/architecture/overview.md](docs/architecture/overview.md)
- **Decisions & Rationale**: [docs/adr/](docs/adr/)
- **API Documentation**: See `/openapi/v1.json` when API running

## Important Notes

### ⚠️ Phase 1 Status
This is the **foundation phase**. Core infrastructure is in place, but many features are not yet implemented. See [PHASE-1-SUMMARY.md](PHASE-1-SUMMARY.md) for what's completed and what's pending.

### 🔒 Privacy & Security
- All user data defaults to PRIVATE
- ZERO monetization - no ads, payments, or premium tiers
- Trauma-informed design - no shame-based mechanics
- No automated enforcement - all moderation requires human review

### ❌ Not an Emergency Service
This platform is NOT an emergency service and does NOT replace professional mental health treatment. If in crisis, contact:
- **National Suicide Prevention Lifeline**: 988 (US)
- **Crisis Text Line**: Text HOME to 741741
- **International Association for Suicide Prevention**: https://www.iasp.info/resources/Crisis_Centres/

## Getting Help

1. Check [PHASE-1-SUMMARY.md](PHASE-1-SUMMARY.md) for implementation status
2. Review [SPECIFICATION.md](SPECIFICATION.md) for requirements
3. Read relevant ADRs in [docs/adr/](docs/adr/)
4. Check GitHub issues for similar problems
5. Open a new issue with detailed steps to reproduce

## Next Steps

1. ✅ **Phase 1a Complete**: Foundation infrastructure
2. 🔄 **Phase 1b**: Implement application shells and API endpoints
3. 🔄 **Phase 1c**: Add features (communities, journals, wellness)
4. 📋 **Phase 2+**: Advanced features, additional services

See [SPECIFICATION.md](SPECIFICATION.md) Section 19 for complete Phase roadmap.

---

**Last Updated**: 2026-09-01  
**Version**: 1.0.0 Foundation Release
