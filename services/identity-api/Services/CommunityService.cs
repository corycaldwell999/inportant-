using Identity.Api.Data;
using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Services
{
    /// <summary>
    /// Service for managing communities
    /// </summary>
    public interface ICommunityService
    {
        /// <summary>
        /// Create a new community
        /// </summary>
        Task<Community> CreateCommunityAsync(
            string name,
            string description,
            string? guidelines,
            string privacyLevel,
            Guid createdBy,
            string? iconUrl = null,
            string? bannerUrl = null);

        /// <summary>
        /// Get community by ID
        /// </summary>
        Task<Community?> GetCommunityByIdAsync(Guid id);

        /// <summary>
        /// Get community by slug
        /// </summary>
        Task<Community?> GetCommunityBySlugAsync(string slug);

        /// <summary>
        /// List all active communities (with pagination)
        /// </summary>
        Task<(List<Community> communities, int total)> ListCommunitiesAsync(
            int page = 1,
            int pageSize = 20,
            string? search = null);

        /// <summary>
        /// Add user to community
        /// </summary>
        Task<CommunityMember> JoinCommunityAsync(Guid communityId, Guid userId);

        /// <summary>
        /// Remove user from community
        /// </summary>
        Task<bool> LeaveCommunityAsync(Guid communityId, Guid userId);

        /// <summary>
        /// Check if user is member of community
        /// </summary>
        Task<bool> IsUserMemberAsync(Guid communityId, Guid userId);

        /// <summary>
        /// Get user's communities (with pagination)
        /// </summary>
        Task<(List<Community> communities, int total)> GetUserCommunitiesAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Get community members (with pagination)
        /// </summary>
        Task<(List<CommunityMember> members, int total)> GetCommunityMembersAsync(
            Guid communityId,
            int page = 1,
            int pageSize = 50);

        /// <summary>
        /// Add moderator to community
        /// </summary>
        Task<CommunityModerator> AddModeratorAsync(
            Guid communityId,
            Guid userId,
            string permissionLevel,
            Guid approvedBy);

        /// <summary>
        /// Remove moderator from community
        /// </summary>
        Task<bool> RemoveModeratorAsync(Guid communityId, Guid userId);

        /// <summary>
        /// Check if user is moderator of community
        /// </summary>
        Task<bool> IsUserModeratorAsync(Guid communityId, Guid userId);
    }

    public class CommunityService : ICommunityService
    {
        private readonly IdentityDbContext _dbContext;
        private readonly ILogger<CommunityService> _logger;

        public CommunityService(IdentityDbContext dbContext, ILogger<CommunityService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Community> CreateCommunityAsync(
            string name,
            string description,
            string? guidelines,
            string privacyLevel,
            Guid createdBy,
            string? iconUrl = null,
            string? bannerUrl = null)
        {
            // Generate URL-friendly slug from name
            var slug = GenerateSlug(name);

            // Check if slug already exists
            var existingCommunity = await _dbContext.Communities
                .FirstOrDefaultAsync(c => c.Slug == slug && c.DeletedAt == null);

            if (existingCommunity != null)
            {
                throw new InvalidOperationException($"Community slug '{slug}' already exists");
            }

            var community = new Community
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                Description = description,
                Guidelines = guidelines,
                CreatedBy = createdBy,
                IconUrl = iconUrl,
                BannerUrl = bannerUrl,
                PrivacyLevel = privacyLevel,
                MemberCount = 1, // Creator is first member
                PostCount = 0,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Communities.Add(community);

            // Add creator as member and moderator
            var creatorMember = new CommunityMember
            {
                Id = Guid.NewGuid(),
                CommunityId = community.Id,
                UserId = createdBy,
                Role = "member",
                JoinedAt = DateTime.UtcNow
            };

            var creatorModerator = new CommunityModerator
            {
                Id = Guid.NewGuid(),
                CommunityId = community.Id,
                UserId = createdBy,
                PermissionLevel = "admin",
                ApprovedAt = DateTime.UtcNow,
                ApprovedBy = createdBy
            };

            _dbContext.CommunityMembers.Add(creatorMember);
            _dbContext.CommunityModerators.Add(creatorModerator);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Community '{Name}' created by user {CreatedBy}",
                name, createdBy);

            return community;
        }

        public async Task<Community?> GetCommunityByIdAsync(Guid id)
        {
            return await _dbContext.Communities
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        }

        public async Task<Community?> GetCommunityBySlugAsync(string slug)
        {
            return await _dbContext.Communities
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.DeletedAt == null);
        }

        public async Task<(List<Community> communities, int total)> ListCommunitiesAsync(
            int page = 1,
            int pageSize = 20,
            string? search = null)
        {
            IQueryable<Community> query = _dbContext.Communities
                .Where(c => c.DeletedAt == null && c.Status == "active")
                .Include(c => c.CreatedByUser)
                .OrderByDescending(c => c.CreatedAt);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchLower) ||
                    c.Description.ToLower().Contains(searchLower));
            }

            query = query.OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync();
            var communities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (communities, total);
        }

        public async Task<CommunityMember> JoinCommunityAsync(Guid communityId, Guid userId)
        {
            // Check if user already member
            var existingMembership = await _dbContext.CommunityMembers
                .FirstOrDefaultAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId);

            if (existingMembership != null)
            {
                return existingMembership;
            }

            var member = new CommunityMember
            {
                Id = Guid.NewGuid(),
                CommunityId = communityId,
                UserId = userId,
                Role = "member",
                JoinedAt = DateTime.UtcNow
            };

            _dbContext.CommunityMembers.Add(member);

            // Increment member count
            var community = await _dbContext.Communities.FindAsync(communityId);
            if (community != null)
            {
                community.MemberCount++;
                community.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} joined community {CommunityId}",
                userId, communityId);

            return member;
        }

        public async Task<bool> LeaveCommunityAsync(Guid communityId, Guid userId)
        {
            var member = await _dbContext.CommunityMembers
                .FirstOrDefaultAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId);

            if (member == null)
            {
                return false;
            }

            _dbContext.CommunityMembers.Remove(member);

            // Decrement member count
            var community = await _dbContext.Communities.FindAsync(communityId);
            if (community != null && community.MemberCount > 0)
            {
                community.MemberCount--;
                community.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} left community {CommunityId}",
                userId, communityId);

            return true;
        }

        public async Task<bool> IsUserMemberAsync(Guid communityId, Guid userId)
        {
            return await _dbContext.CommunityMembers
                .AnyAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId);
        }

        public async Task<(List<Community> communities, int total)> GetUserCommunitiesAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20)
        {
            var query = _dbContext.CommunityMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.Community)
                .Where(c => c.DeletedAt == null)
                .Include(c => c.CreatedByUser)
                .OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync();
            var communities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (communities, total);
        }

        public async Task<(List<CommunityMember> members, int total)> GetCommunityMembersAsync(
            Guid communityId,
            int page = 1,
            int pageSize = 50)
        {
            var query = _dbContext.CommunityMembers
                .Where(m => m.CommunityId == communityId)
                .Include(m => m.User)
                .OrderByDescending(m => m.JoinedAt);

            var total = await query.CountAsync();
            var members = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (members, total);
        }

        public async Task<CommunityModerator> AddModeratorAsync(
            Guid communityId,
            Guid userId,
            string permissionLevel,
            Guid approvedBy)
        {
            // Check if already moderator
            var existingModerator = await _dbContext.CommunityModerators
                .FirstOrDefaultAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId);

            if (existingModerator != null)
            {
                throw new InvalidOperationException("User is already a moderator");
            }

            var moderator = new CommunityModerator
            {
                Id = Guid.NewGuid(),
                CommunityId = communityId,
                UserId = userId,
                PermissionLevel = permissionLevel,
                ApprovedAt = DateTime.UtcNow,
                ApprovedBy = approvedBy
            };

            _dbContext.CommunityModerators.Add(moderator);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} added as moderator to community {CommunityId}",
                userId, communityId);

            return moderator;
        }

        public async Task<bool> RemoveModeratorAsync(Guid communityId, Guid userId)
        {
            var moderator = await _dbContext.CommunityModerators
                .FirstOrDefaultAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId);

            if (moderator == null)
            {
                return false;
            }

            _dbContext.CommunityModerators.Remove(moderator);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} removed as moderator from community {CommunityId}",
                userId, communityId);

            return true;
        }

        public async Task<bool> IsUserModeratorAsync(Guid communityId, Guid userId)
        {
            return await _dbContext.CommunityModerators
                .AnyAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId &&
                    (m.ExpiresAt == null || m.ExpiresAt > DateTime.UtcNow));
        }

        /// <summary>
        /// Generate URL-friendly slug from name
        /// </summary>
        private static string GenerateSlug(string name)
        {
            return name
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("_", "-")
                .Where(c => char.IsLetterOrDigit(c) || c == '-')
                .Aggregate(
                    "",
                    (acc, c) => (acc.EndsWith("-") && c == '-') ? acc : acc + c)
                .Trim('-');
        }
    }
}
