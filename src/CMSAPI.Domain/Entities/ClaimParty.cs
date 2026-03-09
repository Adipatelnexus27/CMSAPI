namespace CMSAPI.Domain.Entities;

public sealed class ClaimParty
{
    public long ClaimPartyId { get; set; }
    public long ClaimId { get; set; }
    public string PartyType { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}
