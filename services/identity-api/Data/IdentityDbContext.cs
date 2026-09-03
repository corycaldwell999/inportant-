using Microsoft.EntityFrameworkCore;
using Identity.Api.Models;

namespace Identity.Api.Data
{
    /// <summary>
    /// Entity Framework Core database context for Identity API
    /// </summary>
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        // DbSets - Authentication
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Device> Devices { get; set; } = null!;
        public DbSet<MFAFactor> MFAFactors { get; set; } = null!;
        public DbSet<ConsentRecord> ConsentRecords { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<TokenRequest> TokenRequests { get; set; } = null!;

        // DbSets - Communities & Posts
        public DbSet<Community> Communities { get; set; } = null!;
        public DbSet<CommunityMember> CommunityMembers { get; set; } = null!;
        public DbSet<CommunityModerator> CommunityModerators { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<PostComment> PostComments { get; set; } = null!;
        public DbSet<PostReaction> PostReactions { get; set; } = null!;
        public DbSet<PostMedia> PostMedia { get; set; } = null!;
        public DbSet<PostRevision> PostRevisions { get; set; } = null!;

        // DbSets - Journal
        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
        public DbSet<JournalFolder> JournalFolders { get; set; } = null!;
        public DbSet<JournalMedia> JournalMedia { get; set; } = null!;
        public DbSet<JournalRevision> JournalRevisions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                
                // Unique email constraint
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
                
                // Timestamps
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationships
                entity.HasMany(e => e.Sessions).WithOne(s => s.User).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Devices).WithOne(d => d.User).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.MFAFactors).WithOne(m => m.User).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.ConsentRecords).WithOne(c => c.User).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.AuditLogs).WithOne(a => a.User).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Session entity
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RefreshToken).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserAgent).IsRequired().HasMaxLength(500);
                
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Index for efficient session lookup
                entity.HasIndex(e => new { e.UserId, e.IsRevoked });
            });

            // Configure Device entity
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DeviceType).IsRequired().HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Unique constraint: user can only have one device with same ID
                entity.HasIndex(e => new { e.UserId, e.DeviceId }).IsUnique();
            });

            // Configure MFAFactor entity
            modelBuilder.Entity<MFAFactor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FactorType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Secret).IsRequired().HasMaxLength(1000);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Index for primary factor lookup
                entity.HasIndex(e => new { e.UserId, e.IsPrimary });
            });

            // Configure ConsentRecord entity
            modelBuilder.Entity<ConsentRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConsentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserAgent).IsRequired().HasMaxLength(500);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Index for consent history lookup
                entity.HasIndex(e => new { e.UserId, e.ConsentType });
            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Resource).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Details).HasMaxLength(4000);
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserAgent).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CorrelationId).IsRequired().HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes for audit trail queries
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.CorrelationId);
            });

            // Configure TokenRequest entity (email verification, password reset)
            modelBuilder.Entity<TokenRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserAgent).IsRequired().HasMaxLength(500);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Index for finding unused tokens
                entity.HasIndex(e => new { e.UserId, e.TokenType, e.UsedAt });
            });

            // Configure Community entity
            modelBuilder.Entity<Community>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Guidelines).HasColumnType("text");
                entity.Property(e => e.PrivacyLevel).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Unique slug per community
                entity.HasIndex(e => e.Slug).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

                // Indexes
                entity.HasIndex(e => e.PrivacyLevel);
                entity.HasIndex(e => e.Status);

                // Relationships
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedCommunities)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Members).WithOne(m => m.Community).HasForeignKey(m => m.CommunityId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Posts).WithOne(p => p.Community).HasForeignKey(p => p.CommunityId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Moderators).WithOne(m => m.Community).HasForeignKey(m => m.CommunityId).OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CommunityMember entity
            modelBuilder.Entity<CommunityMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);

                entity.Property(e => e.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Unique membership
                entity.HasIndex(e => new { e.CommunityId, e.UserId }).IsUnique();

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Role);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.CommunityMemberships)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CommunityModerator entity
            modelBuilder.Entity<CommunityModerator>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PermissionLevel).IsRequired().HasMaxLength(30);

                entity.Property(e => e.ApprovedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Unique moderator role per community
                entity.HasIndex(e => new { e.CommunityId, e.UserId }).IsUnique();

                // Indexes
                entity.HasIndex(e => e.PermissionLevel);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ModeratedCommunities)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Post entity
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Content).IsRequired().HasColumnType("text");
                entity.Property(e => e.PostType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Visibility).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ModerationNotes).HasColumnType("text");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => new { e.CommunityId, e.Status });
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.IsPinned);

                // Relationships
                entity.HasMany(e => e.Comments).WithOne(c => c.Post).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Reactions).WithOne(r => r.Post).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Media).WithOne(m => m.Post).HasForeignKey(m => m.PostId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Revisions).WithOne(r => r.Post).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PostComment entity
            modelBuilder.Entity<PostComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasColumnType("text");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PostId);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.Status);

                // Relationships
                entity.HasMany(e => e.Replies).WithOne(r => r.ParentComment).HasForeignKey(r => r.ParentCommentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Reactions).WithOne(r => r.PostComment).HasForeignKey(r => r.PostCommentId).OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PostReaction entity
            modelBuilder.Entity<PostReaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReactionType).IsRequired().HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // One reaction per user per post/comment
                entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique().HasFilter("\"PostId\" IS NOT NULL");
                entity.HasIndex(e => new { e.PostCommentId, e.UserId }).IsUnique().HasFilter("\"PostCommentId\" IS NOT NULL");
            });

            // Configure PostMedia entity
            modelBuilder.Entity<PostMedia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MediaType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Url).IsRequired();

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PostId);
            });

            // Configure PostRevision entity
            modelBuilder.Entity<PostRevision>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasColumnType("text");
                entity.Property(e => e.EditReason).HasMaxLength(500);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PostId);
            });

            // Configure JournalEntry entity
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Content).IsRequired().HasColumnType("text");
                entity.Property(e => e.MoodDescription).HasMaxLength(500);
                entity.Property(e => e.Tags).HasConversion(
                    v => string.Join(",", v ?? Array.Empty<string>()),
                    v => string.IsNullOrEmpty(v) ? Array.Empty<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes - Journal is private to user, no public queries
                entity.HasIndex(e => new { e.UserId, e.EntryDate });
                entity.HasIndex(e => e.FolderId);

                // Relationships
                entity.HasMany(e => e.Media).WithOne(m => m.Entry).HasForeignKey(m => m.EntryId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Revisions).WithOne(r => r.Entry).HasForeignKey(r => r.EntryId).OnDelete(DeleteBehavior.Cascade);
            });

            // Configure JournalFolder entity
            modelBuilder.Entity<JournalFolder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).HasMaxLength(20);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.UserId);

                // Relationships
                entity.HasMany(e => e.Entries).WithOne(je => je.Folder).HasForeignKey(je => je.FolderId).OnDelete(DeleteBehavior.SetNull);
            });

            // Configure JournalMedia entity
            modelBuilder.Entity<JournalMedia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MediaType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Url).IsRequired();

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.EntryId);
            });

            // Configure JournalRevision entity
            modelBuilder.Entity<JournalRevision>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasColumnType("text");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.EntryId);
            });
        }
    }
}
