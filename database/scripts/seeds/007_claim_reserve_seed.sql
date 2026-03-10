SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (
    VALUES
        ('Claims.Reserve.Read'),
        ('Claims.Reserve.Initial'),
        ('Claims.Reserve.Adjust'),
        ('Claims.Reserve.Approve')
) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Admin');
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Claim Manager');
DECLARE @FinanceRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Finance');

DECLARE @PermissionRoleMap TABLE (RoleId UNIQUEIDENTIFIER, PermissionName NVARCHAR(150));

INSERT INTO @PermissionRoleMap(RoleId, PermissionName)
VALUES
(@AdminRoleId, 'Claims.Reserve.Read'),
(@AdminRoleId, 'Claims.Reserve.Initial'),
(@AdminRoleId, 'Claims.Reserve.Adjust'),
(@AdminRoleId, 'Claims.Reserve.Approve'),

(@ClaimManagerRoleId, 'Claims.Reserve.Read'),
(@ClaimManagerRoleId, 'Claims.Reserve.Initial'),
(@ClaimManagerRoleId, 'Claims.Reserve.Adjust'),
(@ClaimManagerRoleId, 'Claims.Reserve.Approve'),

(@FinanceRoleId, 'Claims.Reserve.Read'),
(@FinanceRoleId, 'Claims.Reserve.Approve');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), prm.RoleId, p.PermissionId
FROM @PermissionRoleMap prm
INNER JOIN dbo.Permissions p ON p.Name = prm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = prm.RoleId AND rp.PermissionId = p.PermissionId
WHERE prm.RoleId IS NOT NULL
  AND rp.RolePermissionId IS NULL;
GO
