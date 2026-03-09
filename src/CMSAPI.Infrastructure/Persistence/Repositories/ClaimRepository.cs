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
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Claim?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClaimId == id && x.IsActive, cancellationToken);
    }

    public async Task<Claim?> GetByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClaimNumber == claimNumber && x.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<Claim>> GetByIdsAsync(IEnumerable<long> claimIds, CancellationToken cancellationToken = default)
    {
        var ids = claimIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbContext.Claims
            .AsNoTracking()
            .Where(x => ids.Contains(x.ClaimId) && x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims.AnyAsync(x => x.ClaimNumber == claimNumber, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(long claimId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims.AnyAsync(x => x.ClaimId == claimId && x.IsActive, cancellationToken);
    }

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        await _dbContext.Claims.AddAsync(claim, cancellationToken);
    }

    public void Update(Claim claim)
    {
        _dbContext.Claims.Update(claim);
    }

    public async Task AddPartyAsync(ClaimParty party, CancellationToken cancellationToken = default)
    {
        await _dbContext.ClaimParties.AddAsync(party, cancellationToken);
    }

    public async Task<ClaimParty?> GetClaimantPartyAsync(long claimId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClaimParties
            .AsNoTracking()
            .Where(x => x.ClaimId == claimId && x.IsActive && x.PartyType == "Claimant")
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimParty>> GetClaimantPartiesByClaimIdsAsync(IEnumerable<long> claimIds, CancellationToken cancellationToken = default)
    {
        var ids = claimIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbContext.ClaimParties
            .AsNoTracking()
            .Where(x => ids.Contains(x.ClaimId) && x.IsActive && x.PartyType == "Claimant")
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddDocumentAsync(ClaimDocument document, CancellationToken cancellationToken = default)
    {
        await _dbContext.ClaimDocuments.AddAsync(document, cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimDocument>> GetDocumentsByClaimIdAsync(long claimId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClaimDocuments
            .AsNoTracking()
            .Where(x => x.ClaimId == claimId && x.IsActive)
            .OrderByDescending(x => x.UploadedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMaxDocumentVersionAsync(long claimId, string fileName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClaimDocuments
            .Where(x => x.ClaimId == claimId && x.FileName == fileName)
            .Select(x => (int?)x.VersionNo)
            .MaxAsync(cancellationToken) ?? 0;
    }

    public async Task AddNoteAsync(ClaimNote note, CancellationToken cancellationToken = default)
    {
        await _dbContext.ClaimNotes.AddAsync(note, cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimNote>> GetNotesByClaimIdAsync(long claimId, string category, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClaimNotes
            .AsNoTracking()
            .Where(x => x.ClaimId == claimId && x.IsActive && x.NoteCategory == category)
            .OrderByDescending(x => x.NotedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RelatedClaimLinkExistsAsync(long claimId, long relatedClaimId, CancellationToken cancellationToken = default)
    {
        var noteText = $"RelatedClaimId:{relatedClaimId}";
        return await _dbContext.ClaimNotes.AnyAsync(
            x => x.ClaimId == claimId && x.IsActive && x.NoteCategory == "RelatedClaim" && x.NoteText == noteText,
            cancellationToken);
    }
}
