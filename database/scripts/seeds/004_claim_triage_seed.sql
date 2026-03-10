SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (
    VALUES
        ('Claims.AssignInvestigator'),
        ('Claims.AssignAdjuster'),
        ('Claims.SetPriority'),
        ('Claims.UpdateStatus'),
        ('Claims.UpdateWorkflow'),
        ('Claims.Assigned.Read')
) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Admin');
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Claim Manager');
DECLARE @InvestigatorRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Investigator');
DECLARE @AdjusterRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Adjuster');

DECLARE @PermissionRoleMap TABLE (RoleId UNIQUEIDENTIFIER, PermissionName NVARCHAR(150));

INSERT INTO @PermissionRoleMap(RoleId, PermissionName)
VALUES
(@AdminRoleId, 'Claims.AssignInvestigator'),
(@AdminRoleId, 'Claims.AssignAdjuster'),
(@AdminRoleId, 'Claims.SetPriority'),
(@AdminRoleId, 'Claims.UpdateStatus'),
(@AdminRoleId, 'Claims.UpdateWorkflow'),
(@AdminRoleId, 'Claims.Assigned.Read'),

(@ClaimManagerRoleId, 'Claims.AssignInvestigator'),
(@ClaimManagerRoleId, 'Claims.AssignAdjuster'),
(@ClaimManagerRoleId, 'Claims.SetPriority'),
(@ClaimManagerRoleId, 'Claims.UpdateStatus'),
(@ClaimManagerRoleId, 'Claims.UpdateWorkflow'),
(@ClaimManagerRoleId, 'Claims.Assigned.Read'),

(@InvestigatorRoleId, 'Claims.UpdateStatus'),
(@InvestigatorRoleId, 'Claims.UpdateWorkflow'),
(@InvestigatorRoleId, 'Claims.Assigned.Read'),

(@AdjusterRoleId, 'Claims.UpdateStatus'),
(@AdjusterRoleId, 'Claims.Assigned.Read');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), prm.RoleId, p.PermissionId
FROM @PermissionRoleMap prm
INNER JOIN dbo.Permissions p ON p.Name = prm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = prm.RoleId AND rp.PermissionId = p.PermissionId
WHERE prm.RoleId IS NOT NULL
  AND rp.RolePermissionId IS NULL;
GO
