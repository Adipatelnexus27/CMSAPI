namespace CMS.Domain.Enums;

public static class ConfigurationTypes
{
    public const string InsuranceProduct = "InsuranceProduct";
    public const string PolicyType = "PolicyType";
    public const string ClaimType = "ClaimType";
    public const string ClaimStatus = "ClaimStatus";

    public static readonly HashSet<string> All =
    [
        InsuranceProduct,
        PolicyType,
        ClaimType,
        ClaimStatus
    ];
}
