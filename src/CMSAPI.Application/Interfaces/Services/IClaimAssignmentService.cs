using CMSAPI.Application.DTOs.ClaimAssignment;

namespace CMSAPI.Application.Interfaces.Services;

public interface IClaimAssignmentService
{
    Task<ClaimAssignmentDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task TriageClaimAsync(long claimId, TriageClaimRequestDto request, long triagedByUserId, string triagedBy, CancellationToken cancellationToken = default);
    Task<AssignmentResultDto> AssignInvestigatorAsync(long claimId, AssignClaimRoleRequestDto request, long assignedByUserId, string assignedBy, CancellationToken cancellationToken = default);
    Task<AssignmentResultDto> AssignAdjusterAsync(long claimId, AssignClaimRoleRequestDto request, long assignedByUserId, string assignedBy, CancellationToken cancellationToken = default);
}
