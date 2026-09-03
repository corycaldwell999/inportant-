# ADR-0001: Monorepo and Service Boundaries

## Status
Accepted

## Context

The platform needs to support multiple applications (web, mobile, admin dashboards), multiple backend services (identity, community, journals, safety, learning, etc.), and shared code across all applications. We evaluated:

1. **Monorepo** - All code in one repository with clear package boundaries
2. **Polyrepo** - Separate repositories for each service/application  
3. **Hybrid** - Mix of monorepo and polyrepo

Key requirements driving this decision:

- Shared TypeScript types and validation to prevent API contract drift
- Consistent design system across web, mobile, and dashboards
- Atomic commits for related changes across packages
- Shared security, privacy, and moderation utilities
- Efficient CI/CD with intelligent caching
- Easy cross-package refactoring
- Clear service boundaries for independent deployment

## Decision

**Adopt a monorepo architecture using Turborepo** with independently deployable backend services.

### Repository Structure

```
trauma-informed-platform/
├── apps/                    # End-user applications
│   ├── web/                # Next.js web application
│   ├── mobile/             # React Native / Expo
│   ├── admin-dashboard/    # Admin interface
│   └── moderator-dashboard/ # Moderation interface
├── services/               # Backend microservices (independently deployable)
│   ├── identity-api/       # Auth, accounts, sessions (Phase 1)
│   ├── user-profile-api/   # Profiles, privacy (Phase 2+)
│   ├── community-api/      # Communities, posts (Phase 2+)
│   └── [other-apis]        # Additional services
├── packages/               # Shared code across all apps
│   ├── types/              # Shared TypeScript types & interfaces
│   ├── validation/         # Zod schemas for runtime validation
│   ├── sdk/                # Typed API client library
│   ├── ui/                 # Accessible design system components
│   ├── config/             # ESLint, Prettier, TypeScript configs
│   ├── security/           # Client-side security utilities
│   └── localization/       # i18n infrastructure & translations
└── infrastructure/         # Docker, Kubernetes, Terraform, etc.
```

### Service Independence

Each backend service in `services/` is independently deployable:

- Each service has its own project file (`.csproj` for .NET)
- Each service has independent versioning
- Docker image built and deployed per-service
- Services scale and update independently
- Service-to-service communication via REST APIs
- Each service owns its database schema

### Turborepo Orchestration

Turborepo provides:

- **Intelligent Caching** - Only rebuild what changed
- **Task Orchestration** - Run lint, test, build in correct dependency order
- **Workspace Filtering** - Target specific packages with `--filter`
- **Remote Caching** - Speed up CI/CD across machines

Example: `npm run build --filter=@trauma-platform/types` builds only types and dependents.

## Rationale

### Why Monorepo?

1. **Type Safety Across Services**
   - Shared `@trauma-platform/types` package prevents API contract drift
   - Single source of truth for domain models
   - Breaking changes caught at compile time

2. **Consistent Patterns**
   - Shared validation schemas (`@trauma-platform/validation`)
   - Unified design system (`@trauma-platform/ui`)
   - Consistent error handling, logging, security

3. **Atomic Refactoring**
   - Changes to types + implementations committed together
   - No need for coordinated versioning across repos
   - Easy to deprecate and migrate APIs

4. **Developer Experience**
   - Clone once, run everything
   - IDE sees entire codebase
   - One CI/CD pipeline configuration
   - Consistent linting and formatting

5. **Operational Efficiency**
   - Single repository to manage and monitor
   - One set of CI/CD secrets and deployment configs
   - Simplified audit trail

### Why Independent Services?

1. **Deployment Flexibility**
   - Update identity service without redeploying community service
   - Scale services based on demand
   - Canary deployments per service

2. **Organizational Scaling**
   - Teams can own specific services
   - Clear responsibility boundaries
   - Reduced merge conflicts

3. **Technology Flexibility**
   - Could add a new service in Python, Go, etc. if needed
   - Services don't force language choice on each other

### Why Not Polyrepo?

- Multiple repositories complicate shared code management
- Type synchronization requires published npm packages and versioning
- Cross-package refactoring very difficult
- Higher operational overhead (N CI/CD pipelines, N secrets, etc.)
- Easy to drift from platform standards

### Why Not Pure Monolith?

- Tight coupling between domains
- Scaling one feature means scaling everything
- Difficult to grant access to specific code
- Deployment risk higher (all-or-nothing)
- Violates separation of concerns

## Consequences

### Positive

- ✅ Shared types prevent silent API contract mismatches
- ✅ Design system changes instantly available everywhere
- ✅ Consistent validation and error handling
- ✅ Single CI/CD pipeline with smart caching
- ✅ Easy to implement features spanning multiple apps
- ✅ Clear audit trail of changes
- ✅ Type-safe refactoring across boundaries

### Negative

- ⚠️ Repository is larger (mitigated by workspace filtering and `.gitignore`)
- ⚠️ Requires discipline to maintain package boundaries
- ⚠️ All developers see all code (deployment controls handle access)
- ⚠️ Git workflow affects all packages (handled by filtering)

### Mitigation Strategies

| Challenge | Mitigation |
|-----------|-----------|
| Large repository | Use `git sparse-checkout` for partial clone |
| Slow operations | Turborepo caching + remote cache in CI/CD |
| Circular dependencies | ESLint rules prohibit cross-service imports |
| Access control | Deployment policies control who deploys what |
| Merge conflicts | Feature branches remain small and focused |

## Implementation Details

### Workspace Management

```bash
# Dependency tree
apps/web depends on @trauma-platform/{types,ui,sdk,validation}
apps/mobile depends on @trauma-platform/{types,ui,sdk,validation}
services/identity-api depends on @trauma-platform/types

# Circular dependency prevention
services/* cannot import from apps/*
services/* cannot import from other services/* directly (via API only)
packages/* can only depend on other packages/*
```

### Package Import Paths

All packages declare path aliases in `tsconfig.json`:

```typescript
// ✅ Correct
import { User } from '@trauma-platform/types';
import { UserProfileSchema } from '@trauma-platform/validation';

// ❌ Wrong (relative imports across packages)
import { User } from '../../../packages/types/src';
```

### CI/CD Implications

- **Turborepo caching** speeds up builds by not rebuilding unchanged packages
- **Remote cache** shares cache across CI machines
- **Dependency graph** ensures correct build order
- **Testing** runs in parallel for independent packages

## Related Decisions

- ADR-0002: Security and Privacy by Default
- Technology Stack decision: Turborepo, TypeScript, Next.js, ASP.NET Core
- Database Strategy: PostgreSQL per logical domain (with shared infrastructure)

## References

- [Turborepo: Monorepo Handbook](https://turborepo.org/docs/handbook)
- [Monorepo.tools Comparison](https://monorepo.tools/)
- [Google's Monorepo Practices](https://www.youtube.com/watch?v=wB02uGqt98M)
- [Stripe's Approach to Monorepos](https://www.youtube.com/watch?v=0IKOi8wT7xA)

## Decision Date

Phase 1 Implementation - 2026-09-01

