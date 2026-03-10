SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (
    VALUES
        ('Audit.Log.Write'),
        ('Audit.Log.Read'),
        ('Audit.Report.Read'),
        ('Audit.Export.Excel'),
        ('Audit.Export.PDF')
) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Admin');
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Claim Manager');
DECLARE @InvestigatorRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Investigator');
DECLARE @AdjusterRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Adjuster');
DECLARE @FinanceRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Finance');
DECLARE @FraudAnalystRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Fraud Analyst');

DECLARE @PermissionRoleMap TABLE (RoleId UNIQUEIDENTIFIER, PermissionName NVARCHAR(150));

INSERT INTO @PermissionRoleMap(RoleId, PermissionName)
VALUES
(@AdminRoleId, 'Audit.Log.Write'),
(@AdminRoleId, 'Audit.Log.Read'),
(@AdminRoleId, 'Audit.Report.Read'),
(@AdminRoleId, 'Audit.Export.Excel'),
(@AdminRoleId, 'Audit.Export.PDF'),

(@ClaimManagerRoleId, 'Audit.Log.Write'),
(@ClaimManagerRoleId, 'Audit.Log.Read'),
(@ClaimManagerRoleId, 'Audit.Report.Read'),
(@ClaimManagerRoleId, 'Audit.Export.Excel'),
(@ClaimManagerRoleId, 'Audit.Export.PDF'),

(@InvestigatorRoleId, 'Audit.Log.Write'),
(@AdjusterRoleId, 'Audit.Log.Write'),
(@FinanceRoleId, 'Audit.Log.Write'),
(@FraudAnalystRoleId, 'Audit.Log.Write');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), prm.RoleId, p.PermissionId
FROM @PermissionRoleMap prm
INNER JOIN dbo.Permissions p ON p.Name = prm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = prm.RoleId AND rp.PermissionId = p.PermissionId
WHERE prm.RoleId IS NOT NULL
  AND rp.RolePermissionId IS NULL;
GO
