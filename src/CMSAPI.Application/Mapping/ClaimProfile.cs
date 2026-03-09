using AutoMapper;
using CMSAPI.Application.DTOs.Claims;
using CMSAPI.Domain.Entities;

namespace CMSAPI.Application.Mapping;

public sealed class ClaimProfile : Profile
{
    public ClaimProfile()
    {
        CreateMap<Claim, ClaimDto>();
    }
}

