SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (
    VALUES
        ('Documents.Read'),
        ('Documents.Upload'),
        ('Documents.Preview')
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
(@AdminRoleId, 'Documents.Read'),
(@AdminRoleId, 'Documents.Upload'),
(@AdminRoleId, 'Documents.Preview'),
(@ClaimManagerRoleId, 'Documents.Read'),
(@ClaimManagerRoleId, 'Documents.Upload'),
(@ClaimManagerRoleId, 'Documents.Preview'),
(@InvestigatorRoleId, 'Documents.Read'),
(@InvestigatorRoleId, 'Documents.Upload'),
(@InvestigatorRoleId, 'Documents.Preview'),
(@AdjusterRoleId, 'Documents.Read'),
(@AdjusterRoleId, 'Documents.Upload'),
(@AdjusterRoleId, 'Documents.Preview'),
(@FinanceRoleId, 'Documents.Read'),
(@FinanceRoleId, 'Documents.Preview'),
(@FraudAnalystRoleId, 'Documents.Read'),
(@FraudAnalystRoleId, 'Documents.Preview');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), prm.RoleId, p.PermissionId
FROM @PermissionRoleMap prm
INNER JOIN dbo.Permissions p ON p.Name = prm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = prm.RoleId AND rp.PermissionId = p.PermissionId
WHERE prm.RoleId IS NOT NULL
  AND rp.RolePermissionId IS NULL;
GO
