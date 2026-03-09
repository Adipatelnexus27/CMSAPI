using CMSAPI.Domain.Enums;

namespace CMSAPI.Application.DTOs.Claims;

public sealed class UpdateClaimStatusRequestDto
{
    public ClaimStatus Status { get; set; }
}

