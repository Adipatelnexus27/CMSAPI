using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Enums;

namespace CMSAPI.Domain.Interfaces;

public interface IClaimAssignmentRepository
{
    Task<Claim?> GetClaimByIdAsync(long claimId, bool asTracking = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Claim>> GetOpenClaimsForDashboardAsync(int maxCount, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimAssignment>> GetCurrentAssignmentsByClaimIdsAsync(IEnumerable<long> claimIds, CancellationToken cancellationToken = default);
    Task DeactivateCurrentAssignmentsByRoleAsync(long claimId, UserRole role, string modifiedBy, CancellationToken cancellationToken = default);
    Task AddAssignmentAsync(ClaimAssignment assignment, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthUser>> GetActiveUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<long, int>> GetActiveAssignmentWorkloadsByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    Task<ClaimNote?> GetLatestNoteByCategoryAsync(long claimId, string category, CancellationToken cancellationToken = default);
    Task AddNoteAsync(ClaimNote note, CancellationToken cancellationToken = default);
}
