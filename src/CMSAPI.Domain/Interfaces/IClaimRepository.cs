using CMSAPI.Domain.Entities;

namespace CMSAPI.Domain.Interfaces;

public interface IClaimRepository
{
    Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Claim?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Claim?> GetByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Claim>> GetByIdsAsync(IEnumerable<long> claimIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(long claimId, CancellationToken cancellationToken = default);
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);
    void Update(Claim claim);

    Task AddPartyAsync(ClaimParty party, CancellationToken cancellationToken = default);
    Task<ClaimParty?> GetClaimantPartyAsync(long claimId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClaimParty>> GetClaimantPartiesByClaimIdsAsync(IEnumerable<long> claimIds, CancellationToken cancellationToken = default);

    Task AddDocumentAsync(ClaimDocument document, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClaimDocument>> GetDocumentsByClaimIdAsync(long claimId, CancellationToken cancellationToken = default);
    Task<int> GetMaxDocumentVersionAsync(long claimId, string fileName, CancellationToken cancellationToken = default);

    Task AddNoteAsync(ClaimNote note, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClaimNote>> GetNotesByClaimIdAsync(long claimId, string category, CancellationToken cancellationToken = default);
    Task<bool> RelatedClaimLinkExistsAsync(long claimId, long relatedClaimId, CancellationToken cancellationToken = default);
}
