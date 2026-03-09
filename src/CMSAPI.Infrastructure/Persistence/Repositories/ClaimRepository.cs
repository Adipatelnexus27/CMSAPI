using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMSAPI.Infrastructure.Persistence.Repositories;

public sealed class ClaimRepository : IClaimRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ClaimRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims.AnyAsync(x => x.ClaimNumber == claimNumber, cancellationToken);
    }

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        await _dbContext.Claims.AddAsync(claim, cancellationToken);
    }

    public void Update(Claim claim)
    {
        _dbContext.Claims.Update(claim);
    }
}

