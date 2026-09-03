using System;
using System.Collections.Generic;

namespace Identity.Api.Models
{
    /// <summary>
    /// Core user entity - represents a registered user on the platform
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool EmailVerified { get; set; }
        public bool IsActive { get; set; }
        public bool RequiresMfa { get; set; }
        public string Role { get; set; } = "user"; // Default role

        // Privacy & consent
        public bool ConsentToTerms { get; set; }
        public bool ConsentToPrivacyPolicy { get; set; }
        public DateTime ConsentedAt { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Relationships - Authentication
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<MFAFactor> MFAFactors { get; set; } = new List<MFAFactor>();
        public ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<TokenRequest> TokenRequests { get; set; } = new List<TokenRequest>();

        // Relationships - Communities
        public ICollection<Community> CreatedCommunities { get; set; } = new List<Community>();
        public ICollection<CommunityMember> CommunityMemberships { get; set; } = new List<CommunityMember>();
        public ICollection<CommunityModerator> ModeratedCommunities { get; set; } = new List<CommunityModerator>();

        // Relationships - Posts
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
        public ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();

        // Relationships - Journal
        public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
        public ICollection<JournalFolder> JournalFolders { get; set; } = new List<JournalFolder>();
    }

    /// <summary>
    /// User session - represents an authenticated login session
    /// </summary>
    public class Session
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public string IpAddress { get; set; } = null!;
        public string UserAgent { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Relationship
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Trusted device - for multi-device session management
    /// </summary>
    public class Device
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DeviceId { get; set; } = null!; // Mobile UDID or browser fingerprint
        public string DeviceName { get; set; } = null!;
        public string DeviceType { get; set; } = null!; // mobile, web, desktop
        public bool IsTrusted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }

        // Relationship
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Multi-factor authentication factor
    /// </summary>
    public class MFAFactor
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FactorType { get; set; } = null!; // "totp", "sms", "email"
        public string Secret { get; set; } = null!; // Encrypted TOTP secret or phone number
        public bool IsVerified { get; set; }
        public bool IsPrimary { get; set; }
        public int? BackupCodesUsed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }

        // Relationship
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Consent record for GDPR compliance
    /// </summary>
    public class ConsentRecord
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ConsentType { get; set; } = null!; // "terms", "privacy", "marketing", "analytics"
        public bool Granted { get; set; }
        public string IpAddress { get; set; } = null!;
        public string UserAgent { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // Relationship
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Audit log - immutable record of all security-relevant actions
    /// </summary>
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string EventType { get; set; } = null!; // "login", "logout", "password_changed", "mfa_enabled", etc.
        public string Resource { get; set; } = null!; // What was acted upon
        public string Action { get; set; } = null!; // What happened
        public string? Details { get; set; } // JSON details
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public string IpAddress { get; set; } = null!;
        public string UserAgent { get; set; } = null!;
        public string CorrelationId { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // Relationship
        public User User { get; set; } = null!;
    }
}
