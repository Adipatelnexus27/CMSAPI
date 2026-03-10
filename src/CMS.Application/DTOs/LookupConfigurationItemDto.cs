namespace CMS.Application.DTOs;

public sealed class LookupConfigurationItemDto
{
    public Guid ConfigurationItemId { get; set; }
    public string ConfigType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
