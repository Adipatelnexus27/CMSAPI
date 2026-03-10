SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (
    VALUES
        ('Claims.Investigation.Read'),
        ('Claims.Investigation.Write'),
        ('Claims.Investigation.DocumentsUpload')
) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Admin');
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Claim Manager');
DECLARE @InvestigatorRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Investigator');
DECLARE @AdjusterRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Adjuster');
DECLARE @FraudAnalystRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Fraud Analyst');

DECLARE @PermissionRoleMap TABLE (RoleId UNIQUEIDENTIFIER, PermissionName NVARCHAR(150));

INSERT INTO @PermissionRoleMap(RoleId, PermissionName)
VALUES
(@AdminRoleId, 'Claims.Investigation.Read'),
(@AdminRoleId, 'Claims.Investigation.Write'),
(@AdminRoleId, 'Claims.Investigation.DocumentsUpload'),

(@ClaimManagerRoleId, 'Claims.Investigation.Read'),
(@ClaimManagerRoleId, 'Claims.Investigation.Write'),
(@ClaimManagerRoleId, 'Claims.Investigation.DocumentsUpload'),

(@InvestigatorRoleId, 'Claims.Investigation.Read'),
(@InvestigatorRoleId, 'Claims.Investigation.Write'),
(@InvestigatorRoleId, 'Claims.Investigation.DocumentsUpload'),

(@AdjusterRoleId, 'Claims.Investigation.Read'),

(@FraudAnalystRoleId, 'Claims.Investigation.Read');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), prm.RoleId, p.PermissionId
FROM @PermissionRoleMap prm
INNER JOIN dbo.Permissions p ON p.Name = prm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = prm.RoleId AND rp.PermissionId = p.PermissionId
WHERE prm.RoleId IS NOT NULL
  AND rp.RolePermissionId IS NULL;
GO
