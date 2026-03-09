using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Enums;
using CMSAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMSAPI.Infrastructure.Persistence.Repositories;

public sealed class ClaimAssignmentRepository : IClaimAssignmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ClaimAssignmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Claim?> GetClaimByIdAsync(long claimId, bool asTracking = false, CancellationToken cancellationToken = default)
    {
        var query = asTracking ? _dbContext.Claims.AsQueryable() : _dbContext.Claims.AsNoTracking();
        return await query.FirstOrDefaultAsync(x => x.ClaimId == claimId && x.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<Claim>> GetOpenClaimsForDashboardAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var closedStatusId = (long)ClaimStatus.Closed;
        return await _dbContext.Claims
            .AsNoTracking()
            .Where(x => x.IsActive && x.CurrentStatusId != closedStatusId)
            .OrderBy(x => x.ReportedDate)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimAssignment>> GetCurrentAssignmentsByClaimIdsAsync(
        IEnumerable<long> claimIds,
        CancellationToken cancellationToken = default)
    {
        var ids = claimIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbContext.ClaimAssignments
            .AsNoTracking()
            .Where(x => ids.Contains(x.ClaimId) && x.IsActive && x.IsCurrent)
            .OrderByDescending(x => x.AssignmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task DeactivateCurrentAssignmentsByRoleAsync(
        long claimId,
        UserRole role,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var roleCode = role.ToString();

        var assignments = await (
            from assignment in _dbContext.ClaimAssignments
            join user in _dbContext.AuthUsers on assignment.AssignedToUserId equals user.UserId
            join authRole in _dbContext.AuthRoles on user.RoleId equals authRole.RoleId
            where assignment.ClaimId == claimId
                && assignment.IsActive
                && assignment.IsCurrent
                && authRole.RoleCode == roleCode
            select assignment
        ).ToListAsync(cancellationToken);

        foreach (var assignment in assignments)
        {
            assignment.IsCurrent = false;
            assignment.ModifiedDate = DateTime.UtcNow;
            assignment.ModifiedBy = modifiedBy;
        }
    }

    public async Task AddAssignmentAsync(ClaimAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _dbContext.ClaimAssignments.AddAsync(assignment, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthUser>> GetActiveUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var roleCode = role.ToString();
        var roleId = await _dbContext.AuthRoles
            .AsNoTracking()
            .Where(x => x.IsActive && x.RoleCode == roleCode)
            .Select(x => (long?)x.RoleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!roleId.HasValue)
        {
            return [];
        }

        return await _dbContext.AuthUsers
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsLocked && x.RoleId == roleId.Value)
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<long, int>> GetActiveAssignmentWorkloadsByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var roleCode = role.ToString();
        var closedStatusId = (long)ClaimStatus.Closed;

        var grouped = await (
            from assignment in _dbContext.ClaimAssignments.AsNoTracking()
            join claim in _dbContext.Claims.AsNoTracking() on assignment.ClaimId equals claim.ClaimId
            join user in _dbContext.AuthUsers.AsNoTracking() on assignment.AssignedToUserId equals user.UserId
            join authRole in _dbContext.AuthRoles.AsNoTracking() on user.RoleId equals authRole.RoleId
            where assignment.IsActive
                && assignment.IsCurrent
                && claim.IsActive
                && claim.CurrentStatusId != closedStatusId
                && authRole.RoleCode == roleCode
            group assignment by assignment.AssignedToUserId
            into bucket
            select new
            {
                UserId = bucket.Key,
                Count = bucket.Count()
            }
        ).ToListAsync(cancellationToken);

        return grouped.ToDictionary(x => x.UserId, x => x.Count);
    }

    public async Task<ClaimNote?> GetLatestNoteByCategoryAsync(long claimId, string category, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClaimNotes
            .AsNoTracking()
            .Where(x => x.ClaimId == claimId && x.IsActive && x.NoteCategory == category)
            .OrderByDescending(x => x.NotedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddNoteAsync(ClaimNote note, CancellationToken cancellationToken = default)
    {
        await _dbContext.ClaimNotes.AddAsync(note, cancellationToken);
    }
}
