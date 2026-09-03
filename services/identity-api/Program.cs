using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Identity.Api.Data;
using Identity.Api.Models;
using Identity.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging with correlation IDs
var environment = builder.Environment;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Register services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Server=localhost;Port=5432;Database=trauma_platform_identity;User Id=postgres;Password=postgres;";
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        npgsqlOptions.CommandTimeout(30);
    }));

// Register application services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
    {
        policy.WithOrigins(
            builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
            new[] { "http://localhost:3000", "http://localhost:3001", "http://localhost:8081" }
        )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configure authentication
var jwtOptions = builder.Configuration.GetSection("Jwt");
var signingKey = jwtOptions["SigningKey"] ?? "trauma-platform-dev-key-minimum-32-chars-required";
var tokenKey = System.Text.Encoding.ASCII.GetBytes(signingKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
        ValidateIssuer = true,
        ValidIssuer = jwtOptions["Issuer"] ?? "https://localhost:7001",
        ValidateAudience = true,
        ValidAudience = jwtOptions["Audience"] ?? "trauma-platform",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
    options.RequireHttpsMetadata = false; // Dev only
});

builder.Services.AddAuthorization();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Initialize database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    try
    {
        if (db.Database.IsRelational() && db.Database.GetPendingMigrations().Any())
        {
            Log.Information("Applying pending migrations...");
            db.Database.Migrate();
            Log.Information("Migrations applied successfully");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to apply migrations");
        // Don't throw - continue startup, may be seeding existing DB
    }
}

// Add request logging middleware with correlation ID
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms [CorrelationId: {CorrelationId}]";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var correlationId = httpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        diagnosticContext.Set("CorrelationId", correlationId);
    };
});

// Add correlation ID middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers.TryGetValue("x-correlation-id", out var value)
        ? value.ToString()
        : Activity.Current?.Id ?? Guid.NewGuid().ToString();

    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["x-correlation-id"] = correlationId;
    await next();
});

app.UseCors("AllowWeb");
app.UseAuthentication();
app.UseAuthorization();

// ============================================================================
// Health & Readiness Endpoints (Kubernetes compatible)
// ============================================================================

app.MapHealthChecks("/health");
app.MapGet("/ready", () => Results.Ok(new { status = "ready", timestamp = DateTimeOffset.UtcNow }))
    .WithName("Readiness")
    .WithOpenApi();

app.MapGet("/live", () => Results.Ok(new { status = "live", timestamp = DateTimeOffset.UtcNow }))
    .WithName("Liveness")
    .WithOpenApi();

// ============================================================================
// System Endpoints
// ============================================================================

app.MapGet("/api/v1/system/info", () =>
{
    return Results.Ok(new
    {
        service = "identity-api",
        version = "1.0.0",
        environment = environment.EnvironmentName,
        timestamp = DateTimeOffset.UtcNow,
        buildTime = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
    });
})
.WithName("SystemInfo")
.WithOpenApi();

// ============================================================================
// OpenAPI / Swagger Documentation
// ============================================================================

app.MapGet("/openapi/v1.json", () => Results.Ok(new
{
    openapi = "3.0.1",
    info = new
    {
        title = "Trauma-Informed Platform - Identity API",
        version = "v1",
        description = "Foundation authentication, authorization, and account management service.",
        contact = new { name = "Platform Team" }
    },
    servers = new object[]
    {
        new { url = "https://localhost:7001", description = "Development" },
        new { url = "https://api.trauma-platform.local", description = "Production" }
    },
    tags = new object[]
    {
        new { name = "Health", description = "Service health and readiness" },
        new { name = "Authentication", description = "User authentication endpoints" },
        new { name = "Authorization", description = "Authorization and permissions" },
        new { name = "Accounts", description = "User account management" }
    },
    paths = new Dictionary<string, object>
    {
        ["/health"] = new { get = new { tags = new[] { "Health" }, summary = "Liveness probe" } },
        ["/ready"] = new { get = new { tags = new[] { "Health" }, summary = "Readiness probe" } },
        ["/live"] = new { get = new { tags = new[] { "Health" }, summary = "Live check" } },
        ["/api/v1/auth/register"] = new { post = new { tags = new[] { "Authentication" }, summary = "Register new user" } },
        ["/api/v1/auth/login"] = new { post = new { tags = new[] { "Authentication" }, summary = "Login user" } },
        ["/api/v1/auth/me"] = new { get = new { tags = new[] { "Authentication" }, summary = "Get current user info" } },
        ["/api/v1/auth/refresh"] = new { post = new { tags = new[] { "Authentication" }, summary = "Refresh access token" } }
    }
}))
.WithName("OpenApiSchema")
.WithOpenApi()
.Produces<object>(200);

app.MapGet("/openapi", () => Results.Redirect("/openapi/v1.json"));

// ============================================================================
// Authentication Endpoints (Phase 1 - Foundation)
// ============================================================================

app.MapPost("/api/v1/auth/register", async (
    [FromBody] RegisterRequest request,
    IdentityDbContext db,
    IPasswordService passwordService,
    IJwtService jwtService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

    // Basic validation
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.DisplayName))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Email, password, and display name are required"
        });
    }

    // Check if user already exists
    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.DeletedAt == null);
    if (existingUser != null)
    {
        return Results.BadRequest(new
        {
            error = "User already exists",
            correlationId,
            message = "This email is already registered"
        });
    }

    try
    {
        // Hash password
        var passwordHash = passwordService.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            DisplayName = request.DisplayName,
            PasswordHash = passwordHash,
            Role = "user",
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        Log.Information("User registered: {UserId} ({Email})", user.Id, user.Email);

        return Results.Created($"/api/v1/users/{user.Id}", new
        {
            id = user.Id,
            email = user.Email,
            displayName = user.DisplayName,
            emailVerified = user.EmailVerified,
            createdAt = user.CreatedAt,
            correlationId,
            message = "Registration successful. Please verify your email to complete setup."
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Registration failed for email: {Email}", request.Email);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("Register")
.WithOpenApi()
.Produces<object>(201)
.Produces<object>(400)
.Produces(500);

app.MapPost("/api/v1/auth/login", async (
    [FromBody] LoginRequest request,
    IdentityDbContext db,
    IPasswordService passwordService,
    IJwtService jwtService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Email and password are required"
        });
    }

    try
    {
        // Find user
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && u.DeletedAt == null);
        if (user == null || !passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            Log.Warning("Failed login attempt for email: {Email}", request.Email);
            return Results.Unauthorized();
        }

        // Generate tokens
        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Email, user.DisplayName ?? "", new[] { user.Role });
        var refreshToken = jwtService.GenerateRefreshToken();

        // Create session
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Sessions.Add(session);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        Log.Information("User logged in: {UserId} ({Email})", user.Id, user.Email);

        return Results.Ok(new
        {
            accessToken,
            refreshToken,
            expiresIn = 3600,
            tokenType = "Bearer",
            user = new
            {
                id = user.Id,
                email = user.Email,
                displayName = user.DisplayName,
                role = user.Role
            },
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Login failed for email: {Email}", request.Email);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("Login")
.WithOpenApi()
.Produces<object>(200)
.Produces(401)
.Produces(500);

app.MapPost("/api/v1/auth/refresh", async (
    [FromBody] RefreshTokenRequest request,
    IdentityDbContext db,
    IJwtService jwtService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

    if (string.IsNullOrWhiteSpace(request.RefreshToken))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Refresh token is required"
        });
    }

    try
    {
        // Find session
        var session = await db.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshToken == request.RefreshToken && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);

        if (session == null)
        {
            return Results.Unauthorized();
        }

        var user = session.User;

        // Generate new access token
        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Email, user.DisplayName ?? "", new[] { user.Role });

        // Generate new refresh token
        var newRefreshToken = jwtService.GenerateRefreshToken();
        session.RefreshToken = newRefreshToken;
        session.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            accessToken,
            refreshToken = newRefreshToken,
            expiresIn = 3600,
            tokenType = "Bearer",
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Token refresh failed");
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("RefreshToken")
.WithOpenApi()
.Produces<object>(200)
.Produces(401)
.Produces(500);

app.MapGet("/api/v1/auth/me", async (HttpContext context, IdentityDbContext db, IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var principal = context.User;

    if (!principal.Identity?.IsAuthenticated ?? false)
    {
        return Results.Unauthorized();
    }

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    try
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
        if (user == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            id = user.Id,
            email = user.Email,
            displayName = user.DisplayName,
            emailVerified = user.EmailVerified,
            role = user.Role,
            lastLoginAt = user.LastLoginAt,
            createdAt = user.CreatedAt,
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to get current user: {UserId}", userId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetCurrentUser")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(401)
.Produces(404)
.Produces(500);

// ============================================================================
// Email Verification & Password Reset Endpoints (Phase 1d)
// ============================================================================

app.MapPost("/api/v1/auth/request-email-verification", async (
    HttpContext context,
    IdentityDbContext db,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var principal = context.User;

    if (!principal.Identity?.IsAuthenticated ?? false)
    {
        return Results.Unauthorized();
    }

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    try
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
        if (user == null)
        {
            return Results.NotFound();
        }

        if (user.EmailVerified)
        {
            return Results.BadRequest(new
            {
                error = "Email already verified",
                correlationId,
                message = "This email is already verified"
            });
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.ToString();

        var token = await tokenService.GenerateEmailVerificationTokenAsync(
            userId, user.Email, ipAddress, userAgent);

        Log.Information("Email verification token generated for user {UserId}", userId);

        return Results.Ok(new
        {
            correlationId,
            message = "Verification email sent. Check your inbox and spam folder.",
            token = token // In production, this would be sent via email, not returned in API
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to request email verification: {UserId}", userId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("RequestEmailVerification")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(400)
.Produces(401)
.Produces(500);

app.MapPost("/api/v1/auth/verify-email", async (
    [FromBody] VerifyEmailRequest request,
    HttpContext context,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var principal = context.User;

    if (!principal.Identity?.IsAuthenticated ?? false)
    {
        return Results.Unauthorized();
    }

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.Token))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Token is required"
        });
    }

    try
    {
        var success = await tokenService.VerifyEmailTokenAsync(request.Token, userId);
        if (!success)
        {
            return Results.BadRequest(new
            {
                error = "Invalid or expired token",
                correlationId,
                message = "The verification token is invalid or has expired. Request a new one."
            });
        }

        Log.Information("Email verified for user {UserId}", userId);

        return Results.Ok(new
        {
            correlationId,
            message = "Email verified successfully!"
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to verify email: {UserId}", userId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("VerifyEmail")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(400)
.Produces(401)
.Produces(500);

app.MapPost("/api/v1/auth/request-password-reset", async (
    [FromBody] RequestPasswordResetRequest request,
    IdentityDbContext db,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Email is required"
        });
    }

    try
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email == request.Email.ToLowerInvariant() &&
            u.DeletedAt == null);

        if (user == null)
        {
            // Don't reveal if email exists (security best practice)
            return Results.Ok(new
            {
                correlationId,
                message = "If an account with this email exists, a password reset link has been sent."
            });
        }

        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "unknown";

        var token = await tokenService.GeneratePasswordResetTokenAsync(
            user.Id, user.Email, ipAddress, userAgent);

        Log.Information("Password reset token generated for user {UserId}", user.Id);

        return Results.Ok(new
        {
            correlationId,
            message = "If an account with this email exists, a password reset link has been sent.",
            token = token // In production, this would be sent via email, not returned in API
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to request password reset");
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("RequestPasswordReset")
.WithOpenApi()
.Produces<object>(200)
.Produces(400)
.Produces(500);

app.MapPost("/api/v1/auth/confirm-password-reset", async (
    [FromBody] ConfirmPasswordResetRequest request,
    IdentityDbContext db,
    ITokenService tokenService,
    IPasswordService passwordService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

    if (string.IsNullOrWhiteSpace(request.Token) ||
        string.IsNullOrWhiteSpace(request.NewPassword) ||
        !Guid.TryParse(request.UserId, out var userId))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Token, user ID, and new password are required"
        });
    }

    try
    {
        var isValidToken = await tokenService.ValidatePasswordResetTokenAsync(request.Token, userId);
        if (!isValidToken)
        {
            return Results.BadRequest(new
            {
                error = "Invalid or expired token",
                correlationId,
                message = "The password reset token is invalid or has expired."
            });
        }

        var user = await db.Users.FindAsync(userId);
        if (user == null || user.DeletedAt != null)
        {
            return Results.NotFound();
        }

        var newPasswordHash = passwordService.HashPassword(request.NewPassword);
        user.PasswordHash = newPasswordHash;
        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Consume the token
        await tokenService.ConsumePasswordResetTokenAsync(request.Token, userId);

        Log.Information("Password reset completed for user {UserId}", userId);

        return Results.Ok(new
        {
            correlationId,
            message = "Password reset successfully. You can now login with your new password."
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to reset password: {UserId}", userId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("ConfirmPasswordReset")
.WithOpenApi()
.Produces<object>(200)
.Produces(400)
.Produces(404)
.Produces(500);

// ============================================================================
// Community Endpoints (Phase 2 - Communities)
// ============================================================================

app.MapPost("/api/v1/communities", async (
    [FromBody] CreateCommunityRequest request,
    HttpContext context,
    ICommunityService communityService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var principal = context.User;

    if (!principal.Identity?.IsAuthenticated ?? false)
    {
        return Results.Unauthorized();
    }

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Description))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Name and description are required"
        });
    }

    try
    {
        var community = await communityService.CreateCommunityAsync(
            request.Name,
            request.Description,
            request.Guidelines,
            request.PrivacyLevel ?? "public",
            userId,
            request.IconUrl,
            request.BannerUrl);

        return Results.Created($"/api/v1/communities/{community.Id}", new
        {
            id = community.Id,
            name = community.Name,
            slug = community.Slug,
            description = community.Description,
            privacyLevel = community.PrivacyLevel,
            memberCount = community.MemberCount,
            createdAt = community.CreatedAt,
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to create community: {UserId}", userId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("CreateCommunity")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(201)
.Produces(400)
.Produces(401)
.Produces(500);

app.MapGet("/api/v1/communities", async (
    ICommunityService communityService,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null) =>
{
    try
    {
        var (communities, total) = await communityService.ListCommunitiesAsync(page, pageSize, search);

        return Results.Ok(new
        {
            data = communities.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                slug = c.Slug,
                description = c.Description,
                memberCount = c.MemberCount,
                privacyLevel = c.PrivacyLevel,
                createdAt = c.CreatedAt
            }),
            pagination = new
            {
                page,
                pageSize,
                total,
                pages = (int)Math.Ceiling(total / (double)pageSize)
            }
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to list communities");
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("ListCommunities")
.WithOpenApi()
.Produces<object>(200)
.Produces(500);

app.MapGet("/api/v1/communities/{id}", async (
    Guid id,
    ICommunityService communityService,
    HttpContext context) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

    try
    {
        var community = await communityService.GetCommunityByIdAsync(id);
        if (community == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            id = community.Id,
            name = community.Name,
            slug = community.Slug,
            description = community.Description,
            guidelines = community.Guidelines,
            memberCount = community.MemberCount,
            postCount = community.PostCount,
            privacyLevel = community.PrivacyLevel,
            status = community.Status,
            createdBy = community.CreatedBy,
            createdAt = community.CreatedAt,
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to get community: {CommunityId}", id);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetCommunityById")
.WithOpenApi()
.Produces<object>(200)
.Produces(404)
.Produces(500);

app.MapPost("/api/v1/communities/{id}/join", async (
    Guid id,
    HttpContext context,
    ICommunityService communityService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var principal = context.User;

    if (!principal.Identity?.IsAuthenticated ?? false)
    {
        return Results.Unauthorized();
    }

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    try
    {
        var member = await communityService.JoinCommunityAsync(id, userId);

        Log.Information("User {UserId} joined community {CommunityId}", userId, id);

        return Results.Ok(new
        {
            message = "Joined community successfully",
            communityId = id,
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to join community: {CommunityId}, {UserId}", id, userId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("JoinCommunity")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(401)
.Produces(500);

app.MapPost("/api/v1/communities/{id}/leave", async (
    Guid id,
    HttpContext context,
    ICommunityService communityService,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var principal = context.User;

    if (!principal.Identity?.IsAuthenticated ?? false)
    {
        return Results.Unauthorized();
    }

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    try
    {
        var success = await communityService.LeaveCommunityAsync(id, userId);
        if (!success)
        {
            return Results.NotFound();
        }

        Log.Information("User {UserId} left community {CommunityId}", userId, id);

        return Results.Ok(new
        {
            message = "Left community successfully",
            communityId = id,
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to leave community: {CommunityId}, {UserId}", id, userId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("LeaveCommunity")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(401)
.Produces(404)
.Produces(500);

// ============================================================================
// Post Endpoints (Phase 2 - Posts & Comments)
// ============================================================================

app.MapPost("/api/v1/posts", async (
    [FromBody] CreatePostRequest request,
    HttpContext context,
    IdentityDbContext db,
    IHttpContextAccessor httpContextAccessor) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var principal = context.User;

    if (!principal.Identity?.IsAuthenticated ?? false)
    {
        return Results.Unauthorized();
    }

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    if (request.CommunityId == Guid.Empty || string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Community and content are required"
        });
    }

    try
    {
        var isMember = await db.CommunityMembers.AnyAsync(m =>
            m.CommunityId == request.CommunityId &&
            m.UserId == userId);

        if (!isMember)
        {
            return Results.Forbid();
        }

        var communityExists = await db.Communities.AnyAsync(c =>
            c.Id == request.CommunityId && c.DeletedAt == null && c.Status == "active");

        if (!communityExists)
        {
            return Results.NotFound();
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            CommunityId = request.CommunityId,
            CreatedBy = userId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            Content = request.Content.Trim(),
            PostType = string.IsNullOrWhiteSpace(request.PostType) ? "text" : request.PostType,
            Visibility = string.IsNullOrWhiteSpace(request.Visibility) ? "community" : request.Visibility,
            AllowComments = request.AllowComments ?? true,
            ReactionCount = 0,
            CommentCount = 0,
            IsPinned = false,
            Status = "published",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        return Results.Created($"/api/v1/posts/{post.Id}", new
        {
            id = post.Id,
            communityId = post.CommunityId,
            title = post.Title,
            content = post.Content,
            postType = post.PostType,
            visibility = post.Visibility,
            allowComments = post.AllowComments,
            createdAt = post.CreatedAt,
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to create community post for user {UserId} in community {CommunityId}", userId, request.CommunityId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("CreatePost")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(201)
.Produces(400)
.Produces(401)
.Produces(403)
.Produces(404)
.Produces(500);

app.MapGet("/api/v1/posts", async (
    [FromQuery] Guid? communityId,
    HttpContext context,
    IdentityDbContext db,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var principal = context.User;

    if (!principal.Identity?.IsAuthenticated ?? false)
    {
        return Results.Unauthorized();
    }

    if (!communityId.HasValue || communityId.Value == Guid.Empty)
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "communityId is required"
        });
    }

    try
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var isMember = await db.CommunityMembers.AnyAsync(m =>
            m.CommunityId == communityId.Value &&
            m.UserId == userId);

        if (!isMember)
        {
            return Results.Forbid();
        }

        var query = db.Posts
            .Where(p => p.CommunityId == communityId.Value && p.DeletedAt == null && p.Status == "published")
            .OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync();
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                id = p.Id,
                communityId = p.CommunityId,
                title = p.Title,
                content = p.Content,
                postType = p.PostType,
                visibility = p.Visibility,
                allowComments = p.AllowComments,
                reactionCount = p.ReactionCount,
                commentCount = p.CommentCount,
                createdAt = p.CreatedAt,
                updatedAt = p.UpdatedAt,
                createdBy = p.CreatedBy
            })
            .ToListAsync();

        return Results.Ok(new
        {
            data = posts,
            pagination = new
            {
                page,
                pageSize,
                total,
                pages = (int)Math.Ceiling(total / (double)pageSize)
            },
            correlationId
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to list community posts for community {CommunityId}", communityId);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("ListPosts")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(400)
.Produces(401)
.Produces(403)
.Produces(500);

app.MapPost("/api/v1/posts/{postId}/comments", async (
    Guid postId,
    [FromBody] CreateCommentRequest request,
    HttpContext context,
    IdentityDbContext db) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var userId = context.User.Claims
        .Where(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)
        .Select(c => Guid.TryParse(c.Value, out var parsedId) ? parsedId : Guid.Empty)
        .FirstOrDefault(id => id != Guid.Empty);

    if (!context.User.Identity?.IsAuthenticated ?? false || userId == Guid.Empty)
    {
        return Results.Unauthorized();
    }

    if (postId == Guid.Empty || string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Post and comment content are required"
        });
    }

    var post = await db.Posts.FirstOrDefaultAsync(p =>
        p.Id == postId && p.DeletedAt == null && p.Status == "published");

    if (post == null)
    {
        return Results.NotFound();
    }

    var isMember = await db.CommunityMembers.AnyAsync(m =>
        m.CommunityId == post.CommunityId && m.UserId == userId);

    if (!isMember)
    {
        return Results.Forbid();
    }

    if (!post.AllowComments)
    {
        return Results.Conflict(new
        {
            error = "Comments disabled",
            correlationId,
            message = "Comments are disabled for this post"
        });
    }

    if (request.ParentCommentId.HasValue)
    {
        var parentExists = await db.PostComments.AnyAsync(c =>
            c.Id == request.ParentCommentId.Value &&
            c.PostId == postId &&
            c.DeletedAt == null &&
            c.Status == "published");

        if (!parentExists)
        {
            return Results.BadRequest(new
            {
                error = "Invalid parent comment",
                correlationId,
                message = "The parent comment does not belong to this post"
            });
        }
    }

    var comment = new PostComment
    {
        Id = Guid.NewGuid(),
        PostId = postId,
        CreatedBy = userId,
        ParentCommentId = request.ParentCommentId,
        Content = request.Content.Trim(),
        Status = "published",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.PostComments.Add(comment);
    post.CommentCount++;
    post.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/posts/{postId}/comments/{comment.Id}", new
    {
        id = comment.Id,
        postId = comment.PostId,
        parentCommentId = comment.ParentCommentId,
        content = comment.Content,
        createdBy = comment.CreatedBy,
        createdAt = comment.CreatedAt,
        correlationId
    });
})
.WithName("CreatePostComment")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(201)
.Produces(400)
.Produces(401)
.Produces(403)
.Produces(404)
.Produces(409);

app.MapGet("/api/v1/posts/{postId}/comments", async (
    Guid postId,
    HttpContext context,
    IdentityDbContext db,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var userId = context.User.Claims
        .Where(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)
        .Select(c => Guid.TryParse(c.Value, out var parsedId) ? parsedId : Guid.Empty)
        .FirstOrDefault(id => id != Guid.Empty);

    if (!context.User.Identity?.IsAuthenticated ?? false || userId == Guid.Empty)
    {
        return Results.Unauthorized();
    }

    var post = await db.Posts.FirstOrDefaultAsync(p =>
        p.Id == postId && p.DeletedAt == null && p.Status == "published");

    if (post == null)
    {
        return Results.NotFound();
    }

    var isMember = await db.CommunityMembers.AnyAsync(m =>
        m.CommunityId == post.CommunityId && m.UserId == userId);

    if (!isMember)
    {
        return Results.Forbid();
    }

    var query = db.PostComments
        .Where(c => c.PostId == postId && c.DeletedAt == null && c.Status == "published")
        .OrderBy(c => c.CreatedAt);
    var total = await query.CountAsync();
    var comments = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new
        {
            id = c.Id,
            postId = c.PostId,
            parentCommentId = c.ParentCommentId,
            content = c.Content,
            createdBy = c.CreatedBy,
            createdAt = c.CreatedAt,
            reactionCount = c.ReactionCount
        })
        .ToListAsync();

    return Results.Ok(new
    {
        data = comments,
        pagination = new
        {
            page,
            pageSize,
            total,
            pages = (int)Math.Ceiling(total / (double)pageSize)
        },
        correlationId
    });
})
.WithName("ListPostComments")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(401)
.Produces(403)
.Produces(404);

app.MapPost("/api/v1/posts/{postId}/reactions", async (
    Guid postId,
    [FromBody] CreateReactionRequest request,
    HttpContext context,
    IdentityDbContext db) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var userId = context.User.Claims
        .Where(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)
        .Select(c => Guid.TryParse(c.Value, out var parsedId) ? parsedId : Guid.Empty)
        .FirstOrDefault(id => id != Guid.Empty);
    var allowedReactions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "i-hear-you", "thinking-of-you", "thank-you-for-sharing", "you-are-not-alone",
        "sending-support", "this-was-helpful", "i-relate", "gentle-encouragement"
    };

    if (!context.User.Identity?.IsAuthenticated ?? false || userId == Guid.Empty)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.ReactionType) || !allowedReactions.Contains(request.ReactionType))
    {
        return Results.BadRequest(new
        {
            error = "Invalid reaction",
            correlationId,
            message = "Choose one of the available supportive reactions"
        });
    }

    var post = await db.Posts.FirstOrDefaultAsync(p =>
        p.Id == postId && p.DeletedAt == null && p.Status == "published");

    if (post == null)
    {
        return Results.NotFound();
    }

    var isMember = await db.CommunityMembers.AnyAsync(m =>
        m.CommunityId == post.CommunityId && m.UserId == userId);

    if (!isMember)
    {
        return Results.Forbid();
    }

    var hasExistingReaction = await db.PostReactions.AnyAsync(r =>
        r.PostId == postId && r.UserId == userId);

    if (hasExistingReaction)
    {
        return Results.Conflict(new
        {
            error = "Reaction already exists",
            correlationId,
            message = "You have already reacted to this post"
        });
    }

    var reaction = new PostReaction
    {
        Id = Guid.NewGuid(),
        PostId = postId,
        UserId = userId,
        ReactionType = request.ReactionType,
        CreatedAt = DateTime.UtcNow
    };

    db.PostReactions.Add(reaction);
    post.ReactionCount++;
    post.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/posts/{postId}/reactions/{reaction.Id}", new
    {
        id = reaction.Id,
        postId,
        reactionType = reaction.ReactionType,
        createdAt = reaction.CreatedAt,
        correlationId
    });
})
.WithName("CreatePostReaction")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(201)
.Produces(400)
.Produces(401)
.Produces(403)
.Produces(404)
.Produces(409);

// ============================================================================
// Journal Endpoints (Phase 2 - Private Journaling)
// ============================================================================

app.MapPost("/api/v1/journal/entries", async (
    [FromBody] CreateJournalEntryRequest request,
    HttpContext context,
    IdentityDbContext db) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var userId = context.User.Claims
        .Where(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)
        .Select(c => Guid.TryParse(c.Value, out var parsedId) ? parsedId : Guid.Empty)
        .FirstOrDefault(id => id != Guid.Empty);

    if (!context.User.Identity?.IsAuthenticated ?? false || userId == Guid.Empty)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.Content) ||
        (request.MoodRating.HasValue && (request.MoodRating < 1 || request.MoodRating > 10)))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Content is required and mood rating must be between 1 and 10"
        });
    }

    var status = string.IsNullOrWhiteSpace(request.Status) ? "saved" : request.Status.ToLowerInvariant();
    if (status is not ("draft" or "saved"))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Status must be draft or saved"
        });
    }

    var entryDate = request.EntryDate ?? DateTime.UtcNow;
    var entry = new JournalEntry
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
        Content = request.Content.Trim(),
        MoodRating = request.MoodRating,
        MoodDescription = string.IsNullOrWhiteSpace(request.MoodDescription) ? null : request.MoodDescription.Trim(),
        Tags = request.Tags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
        Status = status,
        IsPrivate = true,
        EntryDate = entryDate,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.JournalEntries.Add(entry);
    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/journal/entries/{entry.Id}", new
    {
        id = entry.Id,
        title = entry.Title,
        content = entry.Content,
        moodRating = entry.MoodRating,
        moodDescription = entry.MoodDescription,
        tags = entry.Tags,
        status = entry.Status,
        isPrivate = entry.IsPrivate,
        entryDate = entry.EntryDate,
        createdAt = entry.CreatedAt,
        updatedAt = entry.UpdatedAt,
        correlationId
    });
})
.WithName("CreateJournalEntry")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(201)
.Produces(400)
.Produces(401);

app.MapGet("/api/v1/journal/entries", async (
    HttpContext context,
    IdentityDbContext db,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var userId = context.User.Claims
        .Where(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)
        .Select(c => Guid.TryParse(c.Value, out var parsedId) ? parsedId : Guid.Empty)
        .FirstOrDefault(id => id != Guid.Empty);

    if (!context.User.Identity?.IsAuthenticated ?? false || userId == Guid.Empty)
    {
        return Results.Unauthorized();
    }

    var query = db.JournalEntries
        .Where(entry => entry.UserId == userId && entry.DeletedAt == null)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var normalizedSearch = search.Trim().ToLowerInvariant();
        query = query.Where(entry =>
            (entry.Title != null && entry.Title.ToLower().Contains(normalizedSearch)) ||
            entry.Content.ToLower().Contains(normalizedSearch));
    }

    var total = await query.CountAsync();
    var entries = await query
        .OrderByDescending(entry => entry.EntryDate)
        .ThenByDescending(entry => entry.CreatedAt)
        .Skip(Math.Max(0, page - 1) * pageSize)
        .Take(pageSize)
        .Select(entry => new
        {
            id = entry.Id,
            title = entry.Title,
            content = entry.Content,
            moodRating = entry.MoodRating,
            moodDescription = entry.MoodDescription,
            tags = entry.Tags,
            status = entry.Status,
            isPrivate = entry.IsPrivate,
            entryDate = entry.EntryDate,
            createdAt = entry.CreatedAt,
            updatedAt = entry.UpdatedAt
        })
        .ToListAsync();

    return Results.Ok(new
    {
        data = entries,
        pagination = new
        {
            page,
            pageSize,
            total,
            pages = (int)Math.Ceiling(total / (double)pageSize)
        },
        correlationId
    });
})
.WithName("ListJournalEntries")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(401);

app.MapPut("/api/v1/journal/entries/{entryId}", async (
    Guid entryId,
    [FromBody] CreateJournalEntryRequest request,
    HttpContext context,
    IdentityDbContext db) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    var userId = context.User.Claims
        .Where(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)
        .Select(c => Guid.TryParse(c.Value, out var parsedId) ? parsedId : Guid.Empty)
        .FirstOrDefault(id => id != Guid.Empty);

    if (!context.User.Identity?.IsAuthenticated ?? false || userId == Guid.Empty)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.Content) ||
        (request.MoodRating.HasValue && (request.MoodRating < 1 || request.MoodRating > 10)))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Content is required and mood rating must be between 1 and 10"
        });
    }

    var status = string.IsNullOrWhiteSpace(request.Status) ? "saved" : request.Status.ToLowerInvariant();
    if (status is not ("draft" or "saved"))
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            correlationId,
            message = "Status must be draft or saved"
        });
    }

    var entry = await db.JournalEntries.FirstOrDefaultAsync(item =>
        item.Id == entryId && item.UserId == userId && item.DeletedAt == null);

    if (entry == null)
    {
        return Results.NotFound();
    }

    db.JournalRevisions.Add(new JournalRevision
    {
        Id = Guid.NewGuid(),
        EntryId = entry.Id,
        Content = entry.Content,
        MoodRating = entry.MoodRating,
        CreatedAt = DateTime.UtcNow
    });

    entry.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
    entry.Content = request.Content.Trim();
    entry.MoodRating = request.MoodRating;
    entry.MoodDescription = string.IsNullOrWhiteSpace(request.MoodDescription) ? null : request.MoodDescription.Trim();
    entry.Tags = request.Tags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    entry.Status = status;
    entry.IsPrivate = true;
    entry.EntryDate = request.EntryDate ?? entry.EntryDate;
    entry.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        id = entry.Id,
        title = entry.Title,
        content = entry.Content,
        moodRating = entry.MoodRating,
        moodDescription = entry.MoodDescription,
        tags = entry.Tags,
        status = entry.Status,
        isPrivate = entry.IsPrivate,
        entryDate = entry.EntryDate,
        createdAt = entry.CreatedAt,
        updatedAt = entry.UpdatedAt,
        correlationId
    });
})
.WithName("UpdateJournalEntry")
.WithOpenApi()
.RequireAuthorization()
.Produces<object>(200)
.Produces(400)
.Produces(401)
.Produces(404);

app.MapDelete("/api/v1/journal/entries/{entryId}", async (
    Guid entryId,
    HttpContext context,
    IdentityDbContext db) =>
{
    var userId = context.User.Claims
        .Where(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)
        .Select(c => Guid.TryParse(c.Value, out var parsedId) ? parsedId : Guid.Empty)
        .FirstOrDefault(id => id != Guid.Empty);

    if (!context.User.Identity?.IsAuthenticated ?? false || userId == Guid.Empty)
    {
        return Results.Unauthorized();
    }

    var entry = await db.JournalEntries.FirstOrDefaultAsync(item =>
        item.Id == entryId && item.UserId == userId && item.DeletedAt == null);

    if (entry == null)
    {
        return Results.NotFound();
    }

    db.JournalEntries.Remove(entry);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteJournalEntry")
.WithOpenApi()
.RequireAuthorization()
.Produces(204)
.Produces(401)
.Produces(404);

// ============================================================================
// Error Handling
// ============================================================================

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal Server Error",
            correlationId,
            timestamp = DateTimeOffset.UtcNow
        });
    });
});

Log.Information("Trauma-Informed Platform Identity API starting in {Environment}", environment.EnvironmentName);
await app.RunAsync();

// ============================================================================
// Request/Response DTOs
// ============================================================================

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record VerifyEmailRequest(string Token);
public record RequestPasswordResetRequest(string Email);
public record ConfirmPasswordResetRequest(string UserId, string Token, string NewPassword);
public record CreatePostRequest(
    Guid CommunityId,
    string Content,
    string? Title = null,
    string? PostType = null,
    string? Visibility = null,
    bool? AllowComments = true);
public record CreateCommentRequest(string Content, Guid? ParentCommentId = null);
public record CreateReactionRequest(string ReactionType);
public record CreateJournalEntryRequest(
    string Content,
    string? Title = null,
    int? MoodRating = null,
    string? MoodDescription = null,
    string[]? Tags = null,
    string? Status = null,
    DateTime? EntryDate = null);
public record CreateCommunityRequest(
    string Name,
    string Description,
    string? Guidelines = null,
    string? PrivacyLevel = null,
    string? IconUrl = null,
    string? BannerUrl = null);

public partial class Program { }
