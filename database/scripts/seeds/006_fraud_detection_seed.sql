SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (
    VALUES
        ('Fraud.Detect'),
        ('Fraud.Flags.Read'),
        ('Fraud.Flags.Update')
) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Admin');
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Claim Manager');
DECLARE @FraudAnalystRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Fraud Analyst');

DECLARE @PermissionRoleMap TABLE (RoleId UNIQUEIDENTIFIER, PermissionName NVARCHAR(150));

INSERT INTO @PermissionRoleMap(RoleId, PermissionName)
VALUES
(@AdminRoleId, 'Fraud.Detect'),
(@AdminRoleId, 'Fraud.Flags.Read'),
(@AdminRoleId, 'Fraud.Flags.Update'),

(@ClaimManagerRoleId, 'Fraud.Detect'),
(@ClaimManagerRoleId, 'Fraud.Flags.Read'),
(@ClaimManagerRoleId, 'Fraud.Flags.Update'),

(@FraudAnalystRoleId, 'Fraud.Detect'),
(@FraudAnalystRoleId, 'Fraud.Flags.Read'),
(@FraudAnalystRoleId, 'Fraud.Flags.Update');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), prm.RoleId, p.PermissionId
FROM @PermissionRoleMap prm
INNER JOIN dbo.Permissions p ON p.Name = prm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = prm.RoleId AND rp.PermissionId = p.PermissionId
WHERE prm.RoleId IS NOT NULL
  AND rp.RolePermissionId IS NULL;
GO
