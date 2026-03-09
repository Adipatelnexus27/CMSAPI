using AutoMapper;
using CMSAPI.Application.BusinessRules;
using CMSAPI.Application.DTOs.Claims;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using FluentValidation;

namespace CMSAPI.Application.Services;

public sealed class ClaimService : IClaimService
{
    private readonly IClaimRepository _claimRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClaimBusinessRules _businessRules;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateClaimRequestDto> _createValidator;
    private readonly IValidator<UpdateClaimStatusRequestDto> _statusValidator;

    public ClaimService(
        IClaimRepository claimRepository,
        IUnitOfWork unitOfWork,
        ClaimBusinessRules businessRules,
        IMapper mapper,
        IValidator<CreateClaimRequestDto> createValidator,
        IValidator<UpdateClaimStatusRequestDto> statusValidator)
    {
        _claimRepository = claimRepository;
        _unitOfWork = unitOfWork;
        _businessRules = businessRules;
        _mapper = mapper;
        _createValidator = createValidator;
        _statusValidator = statusValidator;
    }

    public async Task<IReadOnlyList<ClaimDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var claims = await _claimRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ClaimDto>>(claims);
    }

    public async Task<ClaimDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetByIdAsync(id, cancellationToken);
        return claim is null ? null : _mapper.Map<ClaimDto>(claim);
    }

    public async Task<ClaimDto> CreateAsync(CreateClaimRequestDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await _businessRules.EnsurePolicyIsEligibleForClaimAsync(request.PolicyNumber.Trim(), request.IncidentDateUtc, cancellationToken);

        var claimNumber = $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        await _businessRules.EnsureClaimNumberIsUniqueAsync(claimNumber, cancellationToken);

        var entity = new Claim
        {
            ClaimNumber = claimNumber,
            PolicyNumber = request.PolicyNumber.Trim(),
            ClaimantName = request.ClaimantName.Trim(),
            IncidentDateUtc = request.IncidentDateUtc,
            ClaimedAmount = request.ClaimedAmount,
            ReservedAmount = request.ReservedAmount,
            Description = request.Description?.Trim()
        };

        await _claimRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ClaimDto>(entity);
    }

    public async Task<ClaimDto> UpdateStatusAsync(Guid id, UpdateClaimStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        await _statusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var claim = await _claimRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim '{id}' was not found.");

        _businessRules.EnsureStatusTransitionAllowed(claim.Status, request.Status);
        claim.Status = request.Status;
        claim.LastModifiedUtc = DateTime.UtcNow;

        _claimRepository.Update(claim);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ClaimDto>(claim);
    }
}
