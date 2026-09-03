namespace Identity.Api.Models;

/// <summary>
/// Represents a one-time-use token for email verification and password reset operations.
/// Tokens are time-limited and can only be used once.
/// </summary>
public class TokenRequest
{
    /// <summary>
    /// Unique identifier for this token request
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID associated with this token request
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of token request: "email_verification" or "password_reset"
    /// </summary>
    public string TokenType { get; set; } = null!;

    /// <summary>
    /// The token string (hashed for security)
    /// </summary>
    public string TokenHash { get; set; } = null!;

    /// <summary>
    /// Email address this token was sent to (for audit trail)
    /// </summary>
    public string EmailAddress { get; set; } = null!;

    /// <summary>
    /// When the token was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the token expires (typically 24 hours after creation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the token was used (null if not yet used)
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// IP address that created the token (for security audit)
    /// </summary>
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// User agent of the request that created the token
    /// </summary>
    public string UserAgent { get; set; } = null!;

    // Foreign key relationship
    public User User { get; set; } = null!;
}
