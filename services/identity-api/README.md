# Identity API

This service provides the initial authentication and authorization foundation for the platform.

## Endpoints

- `/health` - application health
- `/ready` - readiness check
- `/live` - liveness check
- `/openapi` - OpenAPI document
- `/api/v1/system/info` - service metadata
- `/api/v1/auth/me` - authenticated user placeholder
- `/api/v1/auth/config` - OIDC metadata placeholder

## Local run

```bash
Q:/Program Files/dotnet/dotnet.exe run --project services/identity-api/Identity.Api.csproj
```

## Testing

```bash
Q:/Program Files/dotnet/dotnet.exe test services/identity-api/tests/Identity.Api.Tests/Identity.Api.Tests.csproj
```
