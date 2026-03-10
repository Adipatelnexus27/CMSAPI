SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (
    VALUES
        ('Reports.Analytics.Read'),
        ('Reports.Export.Excel'),
        ('Reports.Export.PDF')
) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Admin');
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Claim Manager');
DECLARE @FinanceRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Finance');
DECLARE @FraudAnalystRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Fraud Analyst');

DECLARE @PermissionRoleMap TABLE (RoleId UNIQUEIDENTIFIER, PermissionName NVARCHAR(150));

INSERT INTO @PermissionRoleMap(RoleId, PermissionName)
VALUES
(@AdminRoleId, 'Reports.Analytics.Read'),
(@AdminRoleId, 'Reports.Export.Excel'),
(@AdminRoleId, 'Reports.Export.PDF'),
(@ClaimManagerRoleId, 'Reports.Analytics.Read'),
(@ClaimManagerRoleId, 'Reports.Export.Excel'),
(@ClaimManagerRoleId, 'Reports.Export.PDF'),
(@FinanceRoleId, 'Reports.Analytics.Read'),
(@FinanceRoleId, 'Reports.Export.Excel'),
(@FinanceRoleId, 'Reports.Export.PDF'),
(@FraudAnalystRoleId, 'Reports.Analytics.Read'),
(@FraudAnalystRoleId, 'Reports.Export.Excel'),
(@FraudAnalystRoleId, 'Reports.Export.PDF');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), prm.RoleId, p.PermissionId
FROM @PermissionRoleMap prm
INNER JOIN dbo.Permissions p ON p.Name = prm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = prm.RoleId AND rp.PermissionId = p.PermissionId
WHERE prm.RoleId IS NOT NULL
  AND rp.RolePermissionId IS NULL;
GO
