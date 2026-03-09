using CMSAPI.Application.BusinessRules;
using CMSAPI.Application.DTOs.ClaimAssignment;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Enums;
using CMSAPI.Domain.Interfaces;
using FluentValidation;

namespace CMSAPI.Application.Services;

public sealed class ClaimAssignmentService : IClaimAssignmentService
{
    private const string PriorityNoteCategory = "Priority";
    private const string AssignmentNoteCategory = "Assignment";

    private readonly IClaimAssignmentRepository _assignmentRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClaimBusinessRules _businessRules;
    private readonly IValidator<TriageClaimRequestDto> _triageValidator;
    private readonly IValidator<AssignClaimRoleRequestDto> _assignValidator;

    public ClaimAssignmentService(
        IClaimAssignmentRepository assignmentRepository,
        IClaimRepository claimRepository,
        IPolicyRepository policyRepository,
        IUnitOfWork unitOfWork,
        ClaimBusinessRules businessRules,
        IValidator<TriageClaimRequestDto> triageValidator,
        IValidator<AssignClaimRoleRequestDto> assignValidator)
    {
        _assignmentRepository = assignmentRepository;
        _claimRepository = claimRepository;
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
        _businessRules = businessRules;
        _triageValidator = triageValidator;
        _assignValidator = assignValidator;
    }

    public async Task<ClaimAssignmentDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var claims = await _assignmentRepository.GetOpenClaimsForDashboardAsync(maxCount: 200, cancellationToken);
        var claimIds = claims.Select(x => x.ClaimId).ToList();

        var policies = await _policyRepository.GetPolicyNumbersByIdsAsync(claims.Select(x => x.PolicyId), cancellationToken);
        var claimants = await _claimRepository.GetClaimantPartiesByClaimIdsAsync(claimIds, cancellationToken);
        var claimantLookup = claimants
            .GroupBy(x => x.ClaimId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(p => p.CreatedDate).First().FullName);

        var assignments = await _assignmentRepository.GetCurrentAssignmentsByClaimIdsAsync(claimIds, cancellationToken);
        var investigators = await BuildWorkloadDtosAsync(UserRole.Investigator, cancellationToken);
        var adjusters = await BuildWorkloadDtosAsync(UserRole.Adjuster, cancellationToken);

        var investigatorLookup = investigators.ToDictionary(x => x.UserId);
        var adjusterLookup = adjusters.ToDictionary(x => x.UserId);

        var assignmentLookup = assignments
            .GroupBy(x => x.ClaimId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    AssignmentSummaryDto? investigator = null;
                    AssignmentSummaryDto? adjuster = null;

                    foreach (var assignment in g.OrderByDescending(x => x.AssignmentDate))
                    {
                        if (investigator is null && investigatorLookup.TryGetValue(assignment.AssignedToUserId, out var investigatorUser))
                        {
                            investigator = new AssignmentSummaryDto
                            {
                                UserId = investigatorUser.UserId,
                                DisplayName = investigatorUser.DisplayName,
                                AssignedAtUtc = assignment.AssignmentDate
                            };
                        }

                        if (adjuster is null && adjusterLookup.TryGetValue(assignment.AssignedToUserId, out var adjusterUser))
                        {
                            adjuster = new AssignmentSummaryDto
                            {
                                UserId = adjusterUser.UserId,
                                DisplayName = adjusterUser.DisplayName,
                                AssignedAtUtc = assignment.AssignmentDate
                            };
                        }
                    }

                    return (investigator, adjuster);
                });

        var triageItems = new List<ClaimTriageItemDto>(claims.Count);
        foreach (var claim in claims)
        {
            var priority = await GetClaimPriorityAsync(claim.ClaimId, cancellationToken);
            assignmentLookup.TryGetValue(claim.ClaimId, out var assignedUsers);

            triageItems.Add(new ClaimTriageItemDto
            {
                ClaimId = claim.ClaimId,
                ClaimNumber = claim.ClaimNumber,
                PolicyNumber = policies.GetValueOrDefault(claim.PolicyId, string.Empty),
                ClaimantName = claimantLookup.GetValueOrDefault(claim.ClaimId, "Unknown"),
                LossDateUtc = claim.LossDate,
                StatusName = ToDisplayStatusName(claim.CurrentStatusId),
                Priority = priority,
                EstimatedLossAmount = claim.EstimatedLossAmount,
                Investigator = assignedUsers.investigator,
                Adjuster = assignedUsers.adjuster
            });
        }

        return new ClaimAssignmentDashboardDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            TriageQueue = triageItems
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.LossDateUtc)
                .ToList(),
            Investigators = investigators,
            Adjusters = adjusters
        };
    }

    public async Task TriageClaimAsync(
        long claimId,
        TriageClaimRequestDto request,
        long triagedByUserId,
        string triagedBy,
        CancellationToken cancellationToken = default)
    {
        await _triageValidator.ValidateAndThrowAsync(request, cancellationToken);

        var claim = await _assignmentRepository.GetClaimByIdAsync(claimId, asTracking: true, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim '{claimId}' was not found.");

        var currentStatus = ToClaimStatus(claim.CurrentStatusId);
        if (currentStatus == ClaimStatus.Closed)
        {
            throw new InvalidOperationException("Closed claim cannot be triaged.");
        }

        if (currentStatus == ClaimStatus.New)
        {
            _businessRules.EnsureStatusTransitionAllowed(claim, ClaimStatus.New, ClaimStatus.Triage);
            claim.CurrentStatusId = (long)ClaimStatus.Triage;
            claim.ModifiedDate = DateTime.UtcNow;
            claim.ModifiedBy = triagedBy;
        }

        await _assignmentRepository.AddNoteAsync(new ClaimNote
        {
            ClaimId = claimId,
            NoteCategory = PriorityNoteCategory,
            NoteText = BuildPriorityNoteText(request.Priority, request.Notes),
            NotedDate = DateTime.UtcNow,
            NotedByUserId = triagedByUserId,
            CreatedBy = triagedBy
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<AssignmentResultDto> AssignInvestigatorAsync(
        long claimId,
        AssignClaimRoleRequestDto request,
        long assignedByUserId,
        string assignedBy,
        CancellationToken cancellationToken = default) =>
        AssignByRoleAsync(claimId, UserRole.Investigator, request, assignedByUserId, assignedBy, cancellationToken);

    public Task<AssignmentResultDto> AssignAdjusterAsync(
        long claimId,
        AssignClaimRoleRequestDto request,
        long assignedByUserId,
        string assignedBy,
        CancellationToken cancellationToken = default) =>
        AssignByRoleAsync(claimId, UserRole.Adjuster, request, assignedByUserId, assignedBy, cancellationToken);

    private async Task<AssignmentResultDto> AssignByRoleAsync(
        long claimId,
        UserRole role,
        AssignClaimRoleRequestDto request,
        long assignedByUserId,
        string assignedBy,
        CancellationToken cancellationToken)
    {
        await _assignValidator.ValidateAndThrowAsync(request, cancellationToken);

        var claim = await _assignmentRepository.GetClaimByIdAsync(claimId, asTracking: true, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim '{claimId}' was not found.");

        EnsureAssignmentAllowed(claim);

        var users = await _assignmentRepository.GetActiveUsersByRoleAsync(role, cancellationToken);
        if (users.Count == 0)
        {
            throw new InvalidOperationException($"No active {role} users available for assignment.");
        }

        var workloads = await _assignmentRepository.GetActiveAssignmentWorkloadsByRoleAsync(role, cancellationToken);
        var selectedUser = SelectAssignee(users, workloads, request.UserId);

        await _assignmentRepository.DeactivateCurrentAssignmentsByRoleAsync(claimId, role, assignedBy, cancellationToken);

        var now = DateTime.UtcNow;
        await _assignmentRepository.AddAssignmentAsync(new ClaimAssignment
        {
            ClaimId = claimId,
            AssignedToUserId = selectedUser.UserId,
            AssignedByUserId = assignedByUserId,
            AssignmentDate = now,
            AssignmentReason = request.AssignmentReason?.Trim(),
            IsCurrent = true,
            CreatedBy = assignedBy
        }, cancellationToken);

        await _assignmentRepository.AddNoteAsync(new ClaimNote
        {
            ClaimId = claimId,
            NoteCategory = AssignmentNoteCategory,
            NoteText = $"{role}:{selectedUser.UserId}:{selectedUser.DisplayName}",
            NotedDate = now,
            NotedByUserId = assignedByUserId,
            CreatedBy = assignedBy
        }, cancellationToken);

        var currentStatus = ToClaimStatus(claim.CurrentStatusId);
        if (currentStatus == ClaimStatus.New)
        {
            _businessRules.EnsureStatusTransitionAllowed(claim, ClaimStatus.New, ClaimStatus.Triage);
            claim.CurrentStatusId = (long)ClaimStatus.Triage;
            currentStatus = ClaimStatus.Triage;
        }

        if (currentStatus == ClaimStatus.Triage)
        {
            _businessRules.EnsureStatusTransitionAllowed(claim, ClaimStatus.Triage, ClaimStatus.Assigned);
            claim.CurrentStatusId = (long)ClaimStatus.Assigned;
        }

        claim.ModifiedDate = now;
        claim.ModifiedBy = assignedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var assignedUserWorkload = workloads.GetValueOrDefault(selectedUser.UserId, 0) + 1;
        return new AssignmentResultDto
        {
            ClaimId = claimId,
            ClaimNumber = claim.ClaimNumber,
            AssignmentRole = role.ToString(),
            AssignedToUserId = selectedUser.UserId,
            AssignedToDisplayName = selectedUser.DisplayName,
            AssignedAtUtc = now,
            AssignedUserWorkload = assignedUserWorkload
        };
    }

    private async Task<IReadOnlyList<AssigneeWorkloadDto>> BuildWorkloadDtosAsync(UserRole role, CancellationToken cancellationToken)
    {
        var users = await _assignmentRepository.GetActiveUsersByRoleAsync(role, cancellationToken);
        var workloads = await _assignmentRepository.GetActiveAssignmentWorkloadsByRoleAsync(role, cancellationToken);

        return users
            .Select(user => new AssigneeWorkloadDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                RoleCode = role.ToString(),
                ActiveAssignments = workloads.GetValueOrDefault(user.UserId, 0)
            })
            .OrderBy(x => x.ActiveAssignments)
            .ThenBy(x => x.DisplayName)
            .ToList();
    }

    private static AuthUser SelectAssignee(IReadOnlyList<AuthUser> users, IReadOnlyDictionary<long, int> workloads, long? requestedUserId)
    {
        if (requestedUserId.HasValue)
        {
            var selected = users.FirstOrDefault(x => x.UserId == requestedUserId.Value);
            return selected ?? throw new InvalidOperationException($"User '{requestedUserId}' is not available for assignment.");
        }

        return users
            .OrderBy(x => workloads.GetValueOrDefault(x.UserId, 0))
            .ThenBy(x => x.DisplayName)
            .First();
    }

    private static void EnsureAssignmentAllowed(Claim claim)
    {
        var status = ToClaimStatus(claim.CurrentStatusId);
        if (status is ClaimStatus.Closed or ClaimStatus.Payment or ClaimStatus.Settlement)
        {
            throw new InvalidOperationException($"Assignment is not allowed when claim is in {status} status.");
        }
    }

    private async Task<ClaimPriority> GetClaimPriorityAsync(long claimId, CancellationToken cancellationToken)
    {
        var latestNote = await _assignmentRepository.GetLatestNoteByCategoryAsync(claimId, PriorityNoteCategory, cancellationToken);
        if (latestNote is null)
        {
            return ClaimPriority.Medium;
        }

        return ParsePriorityNote(latestNote.NoteText);
    }

    private static string BuildPriorityNoteText(ClaimPriority priority, string? notes)
    {
        var trimmedNotes = notes?.Trim();
        return string.IsNullOrWhiteSpace(trimmedNotes)
            ? $"Priority:{priority}"
            : $"Priority:{priority}|Notes:{trimmedNotes}";
    }

    private static ClaimPriority ParsePriorityNote(string noteText)
    {
        var marker = "Priority:";
        if (!noteText.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
        {
            return ClaimPriority.Medium;
        }

        var priorityPart = noteText[marker.Length..].Split('|', 2, StringSplitOptions.TrimEntries)[0];
        return Enum.TryParse<ClaimPriority>(priorityPart, true, out var parsed)
            ? parsed
            : ClaimPriority.Medium;
    }

    private string ToDisplayStatusName(long statusId)
    {
        if (!Enum.IsDefined(typeof(ClaimStatus), (int)statusId))
        {
            return $"Status-{statusId}";
        }

        var status = (ClaimStatus)(int)statusId;
        return _businessRules.GetStatusDisplayName(status);
    }

    private static ClaimStatus ToClaimStatus(long statusId)
    {
        if (!Enum.IsDefined(typeof(ClaimStatus), (int)statusId))
        {
            throw new InvalidOperationException($"Unknown claim status id '{statusId}'.");
        }

        return (ClaimStatus)(int)statusId;
    }
}
