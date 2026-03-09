namespace CMSAPI.Application.Security;

public static class Permissions
{
    public const string UsersManage = "Users.Manage";
    public const string ClaimsRead = "Claims.Read";
    public const string ClaimsCreate = "Claims.Create";
    public const string ClaimsAssign = "Claims.Assign";
    public const string ClaimsInvestigate = "Claims.Investigate";
    public const string ClaimsAdjudicate = "Claims.Adjudicate";
    public const string ClaimsPay = "Claims.Pay";
    public const string FraudReview = "Fraud.Review";

    public static readonly string[] All =
    [
        UsersManage,
        ClaimsRead,
        ClaimsCreate,
        ClaimsAssign,
        ClaimsInvestigate,
        ClaimsAdjudicate,
        ClaimsPay,
        FraudReview
    ];
}

