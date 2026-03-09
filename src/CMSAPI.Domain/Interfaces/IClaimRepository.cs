using CMSAPI.Domain.Entities;

namespace CMSAPI.Domain.Interfaces;

public interface IClaimRepository
{
    Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);
    void Update(Claim claim);
}

