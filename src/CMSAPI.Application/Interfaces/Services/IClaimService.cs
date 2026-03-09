using CMSAPI.Application.DTOs.Claims;

namespace CMSAPI.Application.Interfaces.Services;

public interface IClaimService
{
    Task<IReadOnlyList<ClaimDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClaimDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClaimDto> CreateAsync(CreateClaimRequestDto request, CancellationToken cancellationToken = default);
    Task<ClaimDto> UpdateStatusAsync(Guid id, UpdateClaimStatusRequestDto request, CancellationToken cancellationToken = default);
}

