using Identity.Api.Data;
using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Api.Services
{
    /// <summary>
    /// Service for managing email verification and password reset tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate an email verification token for a user
        /// </summary>
        Task<string> GenerateEmailVerificationTokenAsync(Guid userId, string email, string ipAddress, string userAgent);

        /// <summary>
        /// Verify an email verification token and mark email as verified
        /// </summary>
        Task<bool> VerifyEmailTokenAsync(string tokenString, Guid userId);

        /// <summary>
        /// Generate a password reset token for a user
        /// </summary>
        Task<string> GeneratePasswordResetTokenAsync(Guid userId, string email, string ipAddress, string userAgent);

        /// <summary>
        /// Verify a password reset token (doesn't consume it)
        /// </summary>
        Task<bool> ValidatePasswordResetTokenAsync(string tokenString, Guid userId);

        /// <summary>
        /// Consume a password reset token after password change
        /// </summary>
        Task<bool> ConsumePasswordResetTokenAsync(string tokenString, Guid userId);
    }

    public class TokenService : ITokenService
    {
        private readonly IdentityDbContext _dbContext;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IdentityDbContext dbContext, ILogger<TokenService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Generate email verification token
        /// Token is valid for 24 hours
        /// </summary>
        public async Task<string> GenerateEmailVerificationTokenAsync(Guid userId, string email, string ipAddress, string userAgent)
        {
            var tokenString = GenerateRandomToken();
            var tokenHash = HashToken(tokenString);

            var tokenRequest = new TokenRequest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenType = "email_verification",
                TokenHash = tokenHash,
                EmailAddress = email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                UsedAt = null,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _dbContext.TokenRequests.Add(tokenRequest);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Email verification token generated for user {UserId} with email {Email}",
                userId, email);

            return tokenString;
        }

        /// <summary>
        /// Verify email verification token
        /// Marks email as verified if token is valid
        /// </summary>
        public async Task<bool> VerifyEmailTokenAsync(string tokenString, Guid userId)
        {
            var tokenHash = HashToken(tokenString);

            var tokenRequest = await _dbContext.TokenRequests
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId &&
                    t.TokenType == "email_verification" &&
                    t.TokenHash == tokenHash &&
                    t.UsedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow);

            if (tokenRequest == null)
            {
                _logger.LogWarning(
                    "Invalid or expired email verification token for user {UserId}",
                    userId);
                return false;
            }

            // Mark token as used
            tokenRequest.UsedAt = DateTime.UtcNow;

            // Mark user email as verified
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.EmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Email verified for user {UserId}",
                userId);

            return true;
        }

        /// <summary>
        /// Generate password reset token
        /// Token is valid for 24 hours
        /// </summary>
        public async Task<string> GeneratePasswordResetTokenAsync(Guid userId, string email, string ipAddress, string userAgent)
        {
            // Invalidate any existing unused password reset tokens
            var existingTokens = await _dbContext.TokenRequests
                .Where(t =>
                    t.UserId == userId &&
                    t.TokenType == "password_reset" &&
                    t.UsedAt == null)
                .ToListAsync();

            foreach (var token in existingTokens)
            {
                token.UsedAt = DateTime.UtcNow; // Invalidate old tokens
            }

            var tokenString = GenerateRandomToken();
            var tokenHash = HashToken(tokenString);

            var tokenRequest = new TokenRequest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenType = "password_reset",
                TokenHash = tokenHash,
                EmailAddress = email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                UsedAt = null,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _dbContext.TokenRequests.Add(tokenRequest);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Password reset token generated for user {UserId} with email {Email}",
                userId, email);

            return tokenString;
        }

        /// <summary>
        /// Validate password reset token without consuming it
        /// Used to verify token before showing password change form
        /// </summary>
        public async Task<bool> ValidatePasswordResetTokenAsync(string tokenString, Guid userId)
        {
            var tokenHash = HashToken(tokenString);

            var tokenRequest = await _dbContext.TokenRequests
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId &&
                    t.TokenType == "password_reset" &&
                    t.TokenHash == tokenHash &&
                    t.UsedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow);

            if (tokenRequest == null)
            {
                _logger.LogWarning(
                    "Invalid or expired password reset token for user {UserId}",
                    userId);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Consume password reset token after successful password change
        /// </summary>
        public async Task<bool> ConsumePasswordResetTokenAsync(string tokenString, Guid userId)
        {
            var tokenHash = HashToken(tokenString);

            var tokenRequest = await _dbContext.TokenRequests
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId &&
                    t.TokenType == "password_reset" &&
                    t.TokenHash == tokenHash &&
                    t.UsedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow);

            if (tokenRequest == null)
            {
                _logger.LogWarning(
                    "Cannot consume invalid or expired password reset token for user {UserId}",
                    userId);
                return false;
            }

            // Mark token as used
            tokenRequest.UsedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Password reset token consumed for user {UserId}",
                userId);

            return true;
        }

        /// <summary>
        /// Generate a cryptographically secure random token (32 bytes)
        /// </summary>
        private static string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Hash token using SHA256 (tokens stored hashed in database)
        /// </summary>
        private static string HashToken(string token)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
