namespace CMS.Domain.Entities;

public sealed class ClaimRelation
{
    public Guid ClaimRelationId { get; set; }
    public Guid ClaimId { get; set; }
    public Guid RelatedClaimId { get; set; }
    public DateTime LinkedAtUtc { get; set; }
}
