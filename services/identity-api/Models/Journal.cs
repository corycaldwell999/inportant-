namespace Identity.Api.Models;

/// <summary>
/// Represents a user's journal - completely private, never shared or analyzed without explicit consent
/// </summary>
public class JournalEntry
{
    public Guid Id { get; set; }

    /// <summary>
    /// User who owns this journal entry
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Optional folder/collection for organizing entries
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Entry title (optional but recommended)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Entry content (markdown-enabled)
    /// All journal content is sensitive and protected
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Mood at time of writing: 1-10 scale
    /// </summary>
    public int? MoodRating { get; set; }

    /// <summary>
    /// Mood description in user's words
    /// </summary>
    public string? MoodDescription { get; set; }

    /// <summary>
    /// Tags for organizing and searching entries
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Entry status: "draft" or "saved"
    /// </summary>
    public string Status { get; set; } = "saved";

    /// <summary>
    /// Is this entry marked as sensitive/personal?
    /// (For future privacy controls)
    /// </summary>
    public bool IsPrivate { get; set; } = true;

    /// <summary>
    /// When entry was created (user can set custom date)
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// When entry was first written
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When entry was last edited
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete timestamp
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // Foreign key relationships
    public User User { get; set; } = null!;
    public JournalFolder? Folder { get; set; }

    // Navigation properties
    public ICollection<JournalMedia> Media { get; set; } = new List<JournalMedia>();
    public ICollection<JournalRevision> Revisions { get; set; } = new List<JournalRevision>();
}

/// <summary>
/// Represents a folder for organizing journal entries
/// </summary>
public class JournalFolder
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Folder name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Folder description or purpose
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Color code for folder icon (optional)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Entry count (cached for performance)
    /// </summary>
    public int EntryCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Foreign key relationships
    public User User { get; set; } = null!;

    // Navigation properties
    public ICollection<JournalEntry> Entries { get; set; } = new List<JournalEntry>();
}

/// <summary>
/// Represents media attached to a journal entry
/// </summary>
public class JournalMedia
{
    public Guid Id { get; set; }
    public Guid EntryId { get; set; }

    /// <summary>
    /// Media type: "image", "audio", etc.
    /// </summary>
    public string MediaType { get; set; } = null!;

    /// <summary>
    /// URL to media file
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Alt text for accessibility
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public JournalEntry Entry { get; set; } = null!;
}

/// <summary>
/// Represents edit history of a journal entry
/// Helps track progress and changes over time
/// </summary>
public class JournalRevision
{
    public Guid Id { get; set; }
    public Guid EntryId { get; set; }

    /// <summary>
    /// Content at this revision
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Mood at this revision
    /// </summary>
    public int? MoodRating { get; set; }

    public DateTime CreatedAt { get; set; }

    public JournalEntry Entry { get; set; } = null!;
}
