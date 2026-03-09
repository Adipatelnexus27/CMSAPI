namespace CMSAPI.Infrastructure.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";
    public string FromAddress { get; set; } = "noreply@cms.local";
}

