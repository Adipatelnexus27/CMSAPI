namespace CMS.Domain.Enums;

public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string ClaimManager = "Claim Manager";
    public const string Investigator = "Investigator";
    public const string Adjuster = "Adjuster";
    public const string Finance = "Finance";
    public const string FraudAnalyst = "Fraud Analyst";

    public static readonly HashSet<string> All = [Admin, ClaimManager, Investigator, Adjuster, Finance, FraudAnalyst];
}
