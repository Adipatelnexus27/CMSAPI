namespace CMS.API.Middlewares;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute
{
    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
