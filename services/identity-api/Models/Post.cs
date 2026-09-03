namespace Identity.Api.Models;

/// <summary>
/// Represents a post in a community space
/// </summary>
public class Post
{
    public Guid Id { get; set; }

    /// <summary>
    /// Community where post was published
    /// </summary>
    public Guid CommunityId { get; set; }

    /// <summary>
    /// User who created the post
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Post title (for text/discussion posts)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Post content (markdown-enabled)
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Post type: "text", "image", "video", "audio"
    /// </summary>
    public string PostType { get; set; } = "text";

    /// <summary>
    /// Visibility: "public", "community", "anonymous"
    /// </summary>
    public string Visibility { get; set; } = "community";

    /// <summary>
    /// Can this post be commented on? (for sensitive topics, comments might be disabled)
    /// </summary>
    public bool AllowComments { get; set; } = true;

    /// <summary>
    /// Current reaction count (cached for performance)
    /// </summary>
    public int ReactionCount { get; set; } = 0;

    /// <summary>
    /// Current comment count (cached for performance)
    /// </summary>
    public int CommentCount { get; set; } = 0;

    /// <summary>
    /// Is this post pinned/featured in the community?
    /// </summary>
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// Post status: "published", "archived", "hidden" (moderation)
    /// </summary>
    public string Status { get; set; } = "published";

    /// <summary>
    /// Moderation notes (only visible to moderators)
    /// </summary>
    public string? ModerationNotes { get; set; }

    /// <summary>
    /// When post was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When post was last edited
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// When post was deleted (soft delete)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // Foreign key relationships
    public Community Community { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;

    // Navigation properties
    public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    public ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    public ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();
    public ICollection<PostRevision> Revisions { get; set; } = new List<PostRevision>();
}

/// <summary>
/// Represents a comment on a post
/// </summary>
public class PostComment
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Parent comment (for nested replies, null if top-level)
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Comment content (markdown-enabled)
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Current reaction count
    /// </summary>
    public int ReactionCount { get; set; } = 0;

    /// <summary>
    /// Comment status: "published", "hidden" (moderation), "removed" (user deleted)
    /// </summary>
    public string Status { get; set; } = "published";

    /// <summary>
    /// When comment was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When comment was last edited
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Foreign key relationships
    public Post Post { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public PostComment? ParentComment { get; set; }

    // Navigation properties
    public ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
    public ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
}

/// <summary>
/// Represents a supportive reaction to a post or comment (emoji-based)
/// No "dislike" reactions to maintain trauma-informed, supportive environment
/// </summary>
public class PostReaction
{
    public Guid Id { get; set; }

    /// <summary>
    /// Either PostId or CommentId (mutually exclusive)
    /// </summary>
    public Guid? PostId { get; set; }
    public Guid? PostCommentId { get; set; }

    /// <summary>
    /// User who made the reaction
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Reaction emoji: "heart", "support", "grateful", "proud", etc.
    /// </summary>
    public string ReactionType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    // Foreign key relationships
    public Post? Post { get; set; }
    public PostComment? PostComment { get; set; }
    public User User { get; set; } = null!;
}

/// <summary>
/// Represents media attached to a post
/// </summary>
public class PostMedia
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }

    /// <summary>
    /// Media type: "image", "video", "audio"
    /// </summary>
    public string MediaType { get; set; } = null!;

    /// <summary>
    /// URL to media file (could be S3, CloudFront, etc.)
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Alt text for accessibility
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Display order in post (0, 1, 2, ...)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public Post Post { get; set; } = null!;
}

/// <summary>
/// Represents edit history of a post for transparency
/// </summary>
public class PostRevision
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }

    /// <summary>
    /// Content at this revision
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Reason for edit (optional, user-provided)
    /// </summary>
    public string? EditReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public Post Post { get; set; } = null!;
}
