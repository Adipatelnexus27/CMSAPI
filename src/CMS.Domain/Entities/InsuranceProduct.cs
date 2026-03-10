namespace CMS.Domain.Entities;

public sealed class InsuranceProduct
{
    public Guid ConfigurationItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
