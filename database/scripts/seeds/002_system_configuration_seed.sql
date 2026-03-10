SET NOCOUNT ON;

DECLARE @LookupSeed TABLE
(
    ConfigType NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL,
    PRIMARY KEY (ConfigType, Code)
);

INSERT INTO @LookupSeed(ConfigType, Name, Code, Description, DisplayOrder, IsActive)
VALUES
('InsuranceProduct', 'Motor Third Party Liability', 'MTPL', 'Core motor product', 1, 1),
('InsuranceProduct', 'Comprehensive Motor', 'CASCO', 'Comprehensive motor insurance', 2, 1),
('PolicyType', 'Annual', 'ANNUAL', '12-month policy', 1, 1),
('PolicyType', 'Monthly', 'MONTHLY', 'Monthly renewable policy', 2, 1),
('ClaimType', 'Property Damage', 'PROPERTY_DAMAGE', 'Damage to vehicle or property', 1, 1),
('ClaimType', 'Bodily Injury', 'BODILY_INJURY', 'Injury to persons involved in incident', 2, 1),
('ClaimStatus', 'New', 'NEW', 'Claim is newly created', 1, 1),
('ClaimStatus', 'In Review', 'IN_REVIEW', 'Claim is under investigation', 2, 1),
('ClaimStatus', 'Approved', 'APPROVED', 'Claim approved for settlement', 3, 1),
('ClaimStatus', 'Rejected', 'REJECTED', 'Claim rejected after assessment', 4, 1),
('ClaimStatus', 'Closed', 'CLOSED', 'Claim completed and closed', 5, 1);

INSERT INTO dbo.ConfigurationItems
(
    ConfigurationItemId,
    ConfigType,
    Name,
    Code,
    Description,
    DisplayOrder,
    IsActive
)
SELECT NEWID(), ls.ConfigType, ls.Name, ls.Code, ls.Description, ls.DisplayOrder, ls.IsActive
FROM @LookupSeed ls
LEFT JOIN dbo.ConfigurationItems ci ON ci.ConfigType = ls.ConfigType AND ci.Code = ls.Code
WHERE ci.ConfigurationItemId IS NULL;

MERGE dbo.Permissions AS Target
USING (VALUES ('Config.View'), ('Config.Manage')) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @ConfigViewPermissionId UNIQUEIDENTIFIER;
DECLARE @ConfigManagePermissionId UNIQUEIDENTIFIER;
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER;

SELECT @ConfigViewPermissionId = PermissionId FROM dbo.Permissions WHERE Name = 'Config.View';
SELECT @ConfigManagePermissionId = PermissionId FROM dbo.Permissions WHERE Name = 'Config.Manage';
SELECT @AdminRoleId = RoleId FROM dbo.Roles WHERE Name = 'Admin';
SELECT @ClaimManagerRoleId = RoleId FROM dbo.Roles WHERE Name = 'Claim Manager';

IF @AdminRoleId IS NOT NULL AND @ConfigViewPermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @ConfigViewPermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @AdminRoleId, @ConfigViewPermissionId);
END;

IF @AdminRoleId IS NOT NULL AND @ConfigManagePermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @ConfigManagePermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @AdminRoleId, @ConfigManagePermissionId);
END;

IF @ClaimManagerRoleId IS NOT NULL AND @ConfigViewPermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @ClaimManagerRoleId AND PermissionId = @ConfigViewPermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @ClaimManagerRoleId, @ConfigViewPermissionId);
END;

IF @ClaimManagerRoleId IS NOT NULL AND @ConfigManagePermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @ClaimManagerRoleId AND PermissionId = @ConfigManagePermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @ClaimManagerRoleId, @ConfigManagePermissionId);
END;
GO
