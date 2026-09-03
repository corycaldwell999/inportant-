namespace Identity.Api.Models;

public class PrivacySettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ProfileVisibility { get; set; } = "private";
    public bool SearchDiscoverable { get; set; }
    public bool PseudonymousParticipation { get; set; }
    public bool AiPersonalizationOptIn { get; set; }
    public bool ShowReactionCounts { get; set; }
    public bool ShowReadReceipts { get; set; }
    public bool ShowActivityStatus { get; set; }
    public bool ContactSyncEnabled { get; set; }
    public bool LocationSharingEnabled { get; set; }
    public bool EssentialNotificationsEnabled { get; set; } = true;
    public bool NonEssentialNotificationsEnabled { get; set; }
    public bool ReducedMotion { get; set; }
    public bool HighContrast { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserBlock
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BlockedUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserMute
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid MutedUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SafetyPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WarningSigns { get; set; } = "";
    public string CopingStrategies { get; set; } = "";
    public string SafePlaces { get; set; } = "";
    public string ReasonsForLiving { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ICollection<SafetyPlanContact> Contacts { get; set; } = new List<SafetyPlanContact>();
}

public class SafetyPlanContact
{
    public Guid Id { get; set; }
    public Guid SafetyPlanId { get; set; }
    public string Name { get; set; } = null!;
    public string ContactMethod { get; set; } = null!;
    public string ContactValue { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public SafetyPlan SafetyPlan { get; set; } = null!;
}

public class CrisisResource
{
    public Guid Id { get; set; }
    public string RegionCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Phone { get; set; }
    public string? TextNumber { get; set; }
    public string? ChatUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsEmergencyService { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
