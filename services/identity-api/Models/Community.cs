namespace Identity.Api.Models;

/// <summary>
/// Represents a community space where users can gather, share posts, and support each other.
/// Communities are moderated spaces focused on specific topics or support areas.
/// </summary>
public class Community
{
    /// <summary>
    /// Unique identifier for this community
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Community name (displayed to users)
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// URL-friendly slug (e.g., "anxiety-support" from "Anxiety Support")
    /// </summary>
    public string Slug { get; set; } = null!;

    /// <summary>
    /// Detailed description of the community's purpose
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Long-form community guidelines and rules
    /// </summary>
    public string? Guidelines { get; set; }

    /// <summary>
    /// User who created/owns the community
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Community icon/logo URL (optional)
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Community banner/hero image URL (optional)
    /// </summary>
    public string? BannerUrl { get; set; }

    /// <summary>
    /// Privacy level: "public" (anyone can see) or "private" (invite-only)
    /// </summary>
    public string PrivacyLevel { get; set; } = "public";

    /// <summary>
    /// Current member count (cached for performance)
    /// </summary>
    public int MemberCount { get; set; } = 0;

    /// <summary>
    /// Current post count (cached for performance)
    /// </summary>
    public int PostCount { get; set; } = 0;

    /// <summary>
    /// Moderation status: "active", "suspended", or "archived"
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// When community was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last time community was updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete timestamp (null = not deleted)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // Foreign key relationships
    public User CreatedByUser { get; set; } = null!;

    // Navigation properties
    public ICollection<CommunityMember> Members { get; set; } = new List<CommunityMember>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<CommunityModerator> Moderators { get; set; } = new List<CommunityModerator>();
}

/// <summary>
/// Represents a user's membership in a community
/// </summary>
public class CommunityMember
{
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>
    /// Member role: "member" or "moderator" (see CommunityModerator for moderator details)
    /// </summary>
    public string Role { get; set; } = "member";

    /// <summary>
    /// When user joined community
    /// </summary>
    public DateTime JoinedAt { get; set; }

    // Foreign key relationships
    public Community Community { get; set; } = null!;
    public User User { get; set; } = null!;
}

/// <summary>
/// Represents a moderator of a community with elevated permissions
/// </summary>
public class CommunityModerator
{
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>
    /// Moderation permissions level: "moderator", "senior_moderator", "admin"
    /// </summary>
    public string PermissionLevel { get; set; } = "moderator";

    /// <summary>
    /// When user became moderator
    /// </summary>
    public DateTime ApprovedAt { get; set; }

    /// <summary>
    /// User who approved this moderator
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// When moderator status ends (null = no expiration)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    // Foreign key relationships
    public Community Community { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
}
