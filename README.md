# Trauma-Informed Community, Wellness, and Learning Platform

A free, privacy-first, trauma-informed digital community ecosystem focused on mental health education, wellness support, peer connection, and crisis resource navigation. The platform prioritizes psychological safety, user autonomy, and compassionate design over engagement metrics.

**[READ THE COMPLETE SPECIFICATION](docs/SPECIFICATION.md)** - Complete architecture, engineering, security, safety, and delivery specification.

## Status

🚀 **Phase 1: Foundation** - Currently implementing core infrastructure, authentication, and API scaffolding.

## Phase 1 Deliverables

- ✅ Monorepo setup with Turborepo
- ✅ Next.js 14+ web application shell
- ✅ React Native / Expo mobile application shell
- ✅ ASP.NET Core 8+ backend API
- ✅ PostgreSQL 16 + Redis 7 local development environment
- ✅ Shared TypeScript packages:
  - `@trauma-platform/types` - Core domain types (200+ types)
  - `@trauma-platform/validation` - Zod schemas for runtime validation
  - `@trauma-platform/sdk` - Typed API client
  - `@trauma-platform/ui` - Accessible design system components
  - `@trauma-platform/config` - Shared linting and TypeScript configuration
  - `@trauma-platform/security` - Client-side security utilities (planned)
  - `@trauma-platform/localization` - i18n infrastructure (planned)
- ✅ OpenAPI/Swagger configuration
- ✅ Base authentication architecture (identity service)
- ✅ Health check endpoints (/health, /ready)
- ✅ Structured logging with correlation IDs
- ✅ Environment variable templates
- ✅ GitHub Actions CI/CD pipeline
- ✅ ESLint and Prettier configuration
- ✅ TypeScript strict mode everywhere
- ✅ Starter unit and integration tests
- ✅ Architecture documentation and ADRs

## Prerequisites

- **Node.js** 18.0.0+
- **npm** 10.0.0+
- **Docker** and **Docker Compose** (for local database and cache)
- **.NET SDK** 8.0+ (for backend development)
- **Git**

## Quick Start

### 1. Clone and Install

```bash
git clone <repository>
cd trauma-informed-platform
npm install
```

### 2. Configure Environment

Copy the example environment file and update if needed:

```bash
cp .env.example .env
```

Example `.env`:
```env
NODE_ENV=development
NEXT_PUBLIC_API_URL=http://localhost:5000

ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://0.0.0.0:5000

POSTGRES_DB=tip_db
POSTGRES_USER=tip_user
POSTGRES_PASSWORD=tip_password
POSTGRES_PORT=5432

REDIS_HOST=localhost
REDIS_PORT=6379
```

### 3. Start Infrastructure

```bash
npm run docker:up
```

This starts PostgreSQL and Redis. Wait for health checks to pass.

### 4. Run Services

In separate terminals:

```bash
# Backend API (requires .NET SDK)
npm run dev:api

# Web application
npm run dev:web

# Mobile application shell
npm run dev:mobile
```

Services will be available at:
- **Web**: http://localhost:3000
- **API**: http://localhost:5000/api/v1
- **API Docs**: http://localhost:5000/swagger/ui
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

## Development Commands

### Install and Setup

```bash
npm install                      # Install all workspace dependencies
npm run clean                    # Clean all build artifacts and node_modules
```

### Development

```bash
npm run dev                      # Start all services in parallel
npm run dev:web                  # Start Next.js development server
npm run dev:mobile               # Start Expo development server
npm run dev:api                  # Start ASP.NET Core development server
```

### Building

```bash
npm run build                    # Build frontend packages and apps
npm run build:services           # Build backend services
npm run build:all                # Build everything
```

### Testing

```bash
npm run test                     # Run all tests
npm run test:watch               # Run tests in watch mode
npm run test:web                 # Run web app tests
npm run test:api                 # Run API tests
```

### Code Quality

```bash
npm run lint                     # Run ESLint
npm run lint:fix                 # Fix linting issues
npm run format                   # Format code with Prettier
npm run format:check             # Check if code is formatted
npm run type-check               # Run TypeScript type checking
```

### Database and Infrastructure

```bash
npm run docker:up                # Start Docker services
npm run docker:down              # Stop Docker services
npm run docker:logs              # View Docker logs
```

## Project Structure

```
trauma-informed-platform/
├── apps/                        # Applications
│   ├── web/                    # Next.js web application
│   └── mobile/                 # React Native Expo mobile app
├── services/                    # Backend microservices
│   └── identity-api/           # Authentication and identity service
├── packages/                    # Shared packages
│   ├── types/                  # Core TypeScript types
│   ├── validation/             # Zod validation schemas
│   ├── sdk/                    # Typed API client SDK
│   ├── ui/                     # Design system components
│   ├── config/                 # Shared configuration
│   ├── security/               # Security utilities (planned)
│   └── localization/           # i18n infrastructure (planned)
├── infrastructure/
│   ├── docker/                 # Docker Compose
│   ├── kubernetes/             # K8s manifests (Phase 2+)
│   └── database/               # Database migrations
├── docs/
│   ├── architecture/           # Architecture documentation
│   └── adr/                    # Architecture Decision Records
├── tests/                       # E2E and integration tests
├── .github/workflows/          # GitHub Actions CI/CD
└── package.json               # Monorepo root

```

## Architecture

### High-Level Design

The platform uses a modular monorepo architecture with clearly separated concerns:

- **Frontend**: Next.js web app and React Native mobile app
- **Backend**: ASP.NET Core microservices (Identity, User, Community, etc.)
- **Data Layer**: PostgreSQL for persistence, Redis for caching
- **Shared Code**: TypeScript packages for types, validation, and utilities

### Service Architecture

| Service | Purpose |
|---------|---------|
| **Identity API** | Authentication, authorization, account management |
| **User Profile API** | Profiles, privacy settings, preferences (Phase 2+) |
| **Community API** | Communities, posts, comments (Phase 2+) |
| **Journal API** | Private journals and mood tracking (Phase 2+) |
| **Wellness API** | Goals, habits, check-ins (Phase 2+) |
| **Crisis Resources API** | Safety plans and crisis resources (Phase 2+) |
| **Moderation API** | Reports and enforcement (Phase 3+) |
| **Learning API** | Courses and educational content (Phase 2+) |

### Technology Stack

| Layer | Technology |
|-------|-----------|
| **Web** | Next.js 14+, React 18, TypeScript, Tailwind CSS |
| **Mobile** | React Native, Expo, TypeScript |
| **Backend** | ASP.NET Core 8+, C#, Entity Framework Core |
| **Database** | PostgreSQL 16+ |
| **Cache** | Redis 7+ |
| **API** | REST with OpenAPI/Swagger |
| **Testing** | Vitest, xUnit, FluentAssertions, Playwright |
| **CI/CD** | GitHub Actions |
| **Infra** | Docker, Docker Compose |

See [docs/architecture/overview.md](docs/architecture/overview.md) for the complete architecture.

## Key Design Principles

### 1. Privacy-First

- Collect only necessary data
- Default all content to private
- Support data export and deletion
- No profiling, tracking, or behavioral advertising
- Encryption at rest for sensitive data

### 2. Trauma-Informed

- Use compassionate, non-punitive language
- No shame-based mechanics (streaks, ranks, stats)
- Psychological safety over engagement
- Clear boundaries around emergency services
- Human review for high-impact moderation

### 3. Accessible

- WCAG 2.2 AA compliance target
- Full keyboard navigation
- Screen reader support
- High contrast and dark mode options
- Reduced motion support

### 4. Secure

- HTTPS/TLS for all traffic
- Secure password hashing
- MFA for staff accounts
- Regular security audits
- Vulnerability scanning in CI/CD
- OWASP Top 10 protections

### 5. Maintainable

- Monorepo with shared packages
- TypeScript strict mode
- Comprehensive testing
- Architecture decision records
- Clear separation of concerns

## API Documentation

API documentation is available at:
- **Swagger UI**: http://localhost:5000/swagger/ui
- **OpenAPI JSON**: http://localhost:5000/swagger/v1/swagger.json

### Authentication

The Identity API provides OAuth 2.0 and OpenID Connect compatible authentication:

```bash
# Register
POST /api/v1/auth/register

# Login
POST /api/v1/auth/login

# Logout
POST /api/v1/auth/logout

# Refresh token
POST /api/v1/auth/refresh
```

### Example Request

```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123"
  }'
```

### Response Format

All API responses follow a consistent envelope:

```json
{
  "data": {
    "id": "uuid",
    "email": "user@example.com",
    "displayName": "User Name"
  },
  "meta": {
    "requestId": "correlation-id",
    "timestamp": "2024-01-01T00:00:00Z"
  }
}
```

## Testing

### Unit Tests

```bash
npm run test
npm run test:watch
```

### Integration Tests

Integration tests for APIs are included in each service:

```bash
npm run test:api
```

### End-to-End Tests

E2E tests with Playwright (Phase 2+):

```bash
npm run test:e2e
```

### Test Coverage

Coverage reports are generated to `coverage/`:

```bash
npm run test -- --coverage
```

## Security

### Dependency Management

Keep dependencies up-to-date:

```bash
npm audit                        # Check for vulnerabilities
npm audit fix                    # Auto-fix if possible
```

### Secrets Management

- Never commit `.env` file or secrets
- Use `.env.example` as template
- Production secrets in external manager (Vault, AWS Secrets Manager, etc.)
- Environment variables for all secrets in CI/CD

### OWASP Protection

The platform includes protections against:
- ✅ SQL Injection (parameterized queries)
- ✅ Cross-Site Scripting (input validation, CSP headers)
- ✅ Cross-Site Request Forgery (CSRF tokens)
- ✅ Broken Authentication (secure sessions, MFA)
- ✅ Sensitive Data Exposure (encryption, HTTPS)
- ✅ XXE Attacks (XML parsing security)
- ✅ Broken Access Control (RBAC, permission checks)
- ✅ Security Misconfiguration (security headers, safe defaults)
- ✅ Using Components with Known Vulnerabilities (dependency scanning)
- ✅ Insufficient Logging & Monitoring (structured logging)

## Performance

### Caching

Redis is used for:
- Session storage
- Rate limit counters
- Frequently accessed data

### Database Optimization

- Indexes on frequently queried columns
- Query optimization and analysis
- Connection pooling
- Read replicas for scaling

### CDN

Static assets are served via CDN in production.

## Monitoring and Observability

### Logging

Structured JSON logging with correlation IDs for request tracing:

```csharp
logger.LogInformation(
  "User registered. RequestId: {RequestId}",
  httpContext.TraceIdentifier
);
```

### Health Checks

- `GET /health` - Basic health status
- `GET /ready` - Readiness for traffic

### Metrics (Future)

OpenTelemetry integration for:
- Request rate and latency
- Error rates
- Business metrics

## Deployment

### Docker Build

```bash
docker build -f services/identity-api/Dockerfile .
docker run -p 5000:5000 <image-id>
```

### Docker Compose

```bash
docker-compose -f infrastructure/docker/docker-compose.yml up
```

See [docs/operations/](docs/operations/) for production deployment guides.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Code Style

- ESLint for JavaScript/TypeScript
- Prettier for formatting
- StyleLint for CSS (Phase 2+)
- C# code style per editorconfig

```bash
npm run lint:fix
npm run format
```

### Git Workflow

1. Create feature branch: `git checkout -b feature/description`
2. Commit changes: `git commit -am "Clear description"`
3. Push: `git push origin feature/description`
4. Open Pull Request

All PRs must pass:
- ✅ Linting
- ✅ Type checking
- ✅ Tests
- ✅ Build
- ✅ Security scanning

## Support and Resources

- **Docs**: [docs/](docs/)
- **Architecture**: [docs/architecture/](docs/architecture/)
- **ADRs**: [docs/adr/](docs/adr/)
- **API Docs**: http://localhost:5000/swagger/ui (when running)

## Roadmap

### Phase 1 ✅ In Progress
Foundation, monorepo, identity, health checks

### Phase 2 (Next)
Profiles, communities, journals, wellness, learning, crisis resources

### Phase 3
Messaging, search, advanced moderation, analytics

### Phase 4+
AI features, mobile optimization, scaling, enterprise

## License

MIT License - See [LICENSE](LICENSE) for details.

## Important Notices

This platform:
- **Is NOT** a replacement for emergency services
- **Is NOT** a crisis hotline
- **Is NOT** a substitute for therapy
- **Is NOT** a diagnostic tool
- **Is NOT** a medical service

Users experiencing immediate crisis should contact emergency services or their local crisis line.

## Acknowledgments

Built with principles from:
- Trauma-informed care research
- Privacy by design
- Accessible design standards
- Open source best practices

