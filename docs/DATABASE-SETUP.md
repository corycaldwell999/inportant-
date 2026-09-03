# Database Setup and Migration Guide

## Overview

The Trauma-Informed Platform uses PostgreSQL 16 with Entity Framework Core 8 for data persistence. The Identity API manages all user authentication, authorization, and account data.

## Prerequisites

- Docker and Docker Compose installed
- .NET 8 SDK installed
- dotnet-ef CLI tool (installed locally in workspace)
- Node.js and npm

## Database Architecture

### PostgreSQL Configuration

- **Host**: localhost
- **Port**: 5432
- **Database**: trauma_platform_identity
- **Default User**: postgres
- **Default Password**: postgres (dev only - change in production)
- **Persistence**: Docker volume (`postgres_data`)

### Entity Framework Core

- **Version**: 8.0.16
- **Database Provider**: Npgsql.EntityFrameworkCore.PostgreSQL
- **Migrations Location**: `services/identity-api/Migrations/`

## Starting the Database

### 1. Using Docker Compose

```bash
# Start PostgreSQL and Redis
npm run docker:up

# View logs
npm run docker:logs

# Stop services
npm run docker:down
```

### 2. Manual Docker Commands

```bash
# Start only PostgreSQL
docker-compose -f infrastructure/docker/docker-compose.yml up -d postgres

# Check status
docker-compose -f infrastructure/docker/docker-compose.yml ps

# View PostgreSQL logs
docker-compose -f infrastructure/docker/docker-compose.yml logs postgres
```

## Database Migrations

### Creating a New Migration

```bash
cd services/identity-api

# Create migration
dotnet tool run dotnet-ef migrations add YourMigrationName --context IdentityDbContext

# View pending migrations
dotnet tool run dotnet-ef migrations list
```

### Applying Migrations

The Identity API automatically applies pending migrations on startup:

```csharp
// From Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    if (db.Database.GetPendingMigrations().Any())
    {
        db.Database.Migrate();
    }
}
```

### Manual Migration Application

```bash
cd services/identity-api

# Apply all pending migrations
dotnet tool run dotnet-ef database update

# Revert to specific migration
dotnet tool run dotnet-ef database update PreviousMigrationName

# Remove last migration
dotnet tool run dotnet-ef migrations remove
```

## Database Schema

### Core Tables

#### Users
- **Columns**: id, email, display_name, password_hash, role, email_verified, mfa_enabled, created_at, updated_at, deleted_at
- **Indexes**: email (unique, filtered on soft delete)
- **Relationships**: Sessions, Devices, MFAFactors, ConsentRecords, AuditLogs

#### Sessions
- **Columns**: id, user_id, refresh_token, expires_at, is_revoked, ip_address, user_agent, created_at, updated_at
- **Indexes**: (user_id, is_revoked) for efficient lookups
- **Relationships**: User

#### Devices
- **Columns**: id, user_id, device_id, device_name, device_type, is_trusted, created_at, updated_at, last_used_at
- **Indexes**: (user_id, device_id) unique constraint
- **Relationships**: User

#### MFAFactors
- **Columns**: id, user_id, factor_type, secret, is_verified, is_primary, created_at, verified_at
- **Indexes**: (user_id, is_primary)
- **Relationships**: User

#### ConsentRecords
- **Columns**: id, user_id, consent_type, granted, ip_address, user_agent, created_at
- **Indexes**: (user_id, consent_type)
- **Relationships**: User

#### AuditLogs
- **Columns**: id, user_id, event_type, resource, action, details, success, failure_reason, ip_address, user_agent, correlation_id, created_at
- **Indexes**: (user_id, created_at), event_type, correlation_id
- **Relationships**: User

## Connection String

The connection string is configured in `Program.cs`:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Server=localhost;Port=5432;Database=trauma_platform_identity;User Id=postgres;Password=postgres;";
```

To customize, set `ConnectionStrings:DefaultConnection` in `appsettings.json` or environment variables.

## Configuration Files

### appsettings.json
Default configuration for production-like environments.

### appsettings.Development.json
Development-specific configuration with logging, debug info, and local database settings.

## Testing Database Connectivity

```bash
# Connect to PostgreSQL via psql
docker exec -it trauma-platform-postgres psql -U postgres -d trauma_platform_identity

# List tables
\dt

# View schema
\d+ users

# Exit
\q
```

## Troubleshooting

### Connection Refused

1. Verify PostgreSQL container is running: `npm run docker:logs`
2. Check firewall rules allow port 5432
3. Verify connection string in `appsettings.json`

### Migration Errors

1. Check for conflicting migrations: `dotnet tool run dotnet-ef migrations list`
2. Verify DbContext is properly configured
3. Check that User Id and Password are correct

### Schema Mismatch

1. View current database schema: `dotnet tool run dotnet-ef dbcontext info`
2. Revert to known-good state: `npm run docker:down && npm run docker:up`
3. Re-apply migrations

## Security Considerations

- **Passwords**: All user passwords are hashed using bcrypt with 12-round work factor
- **Tokens**: Refresh tokens are cryptographically secure random strings stored in database
- **Audit Logging**: All authentication events are logged with IP, user agent, and correlation ID
- **Soft Deletes**: Users are marked deleted rather than removed from database
- **Unique Constraints**: Email is unique per non-deleted user

## Next Steps

- Implement password reset flow
- Add email verification logic
- Implement MFA setup and verification
- Create user profile endpoints
- Add comprehensive integration tests
- Set up database backups for production

---

**Last Updated**: 2026-09-02  
**Version**: 1.0.0  
**Status**: Phase 1 - Foundation Complete
