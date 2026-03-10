SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (
    VALUES
        ('Claims.Settlement.Calculate'),
        ('Claims.Settlement.Read'),
        ('Payments.Request'),
        ('Payments.Read'),
        ('Payments.Approve'),
        ('Payments.Status.Update')
) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Admin');
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Claim Manager');
DECLARE @FinanceRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Finance');
DECLARE @AdjusterRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Adjuster');

DECLARE @PermissionRoleMap TABLE (RoleId UNIQUEIDENTIFIER, PermissionName NVARCHAR(150));

INSERT INTO @PermissionRoleMap(RoleId, PermissionName)
VALUES
(@AdminRoleId, 'Claims.Settlement.Calculate'),
(@AdminRoleId, 'Claims.Settlement.Read'),
(@AdminRoleId, 'Payments.Request'),
(@AdminRoleId, 'Payments.Read'),
(@AdminRoleId, 'Payments.Approve'),
(@AdminRoleId, 'Payments.Status.Update'),

(@ClaimManagerRoleId, 'Claims.Settlement.Calculate'),
(@ClaimManagerRoleId, 'Claims.Settlement.Read'),
(@ClaimManagerRoleId, 'Payments.Request'),
(@ClaimManagerRoleId, 'Payments.Read'),

(@FinanceRoleId, 'Claims.Settlement.Read'),
(@FinanceRoleId, 'Payments.Request'),
(@FinanceRoleId, 'Payments.Read'),
(@FinanceRoleId, 'Payments.Approve'),
(@FinanceRoleId, 'Payments.Status.Update'),

(@AdjusterRoleId, 'Claims.Settlement.Calculate'),
(@AdjusterRoleId, 'Claims.Settlement.Read'),
(@AdjusterRoleId, 'Payments.Read');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), prm.RoleId, p.PermissionId
FROM @PermissionRoleMap prm
INNER JOIN dbo.Permissions p ON p.Name = prm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = prm.RoleId AND rp.PermissionId = p.PermissionId
WHERE prm.RoleId IS NOT NULL
  AND rp.RolePermissionId IS NULL;
GO
