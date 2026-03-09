using CMSAPI.Application.DTOs.Policies;

namespace CMSAPI.Application.Interfaces.Services;

public interface IPolicyService
{
    Task<IReadOnlyList<PolicyDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PolicyDto?> GetByIdAsync(long policyId, CancellationToken cancellationToken = default);
    Task<PolicyDto?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default);
    Task<PolicyDto> CreateAsync(CreatePolicyRequestDto request, CancellationToken cancellationToken = default);
}

