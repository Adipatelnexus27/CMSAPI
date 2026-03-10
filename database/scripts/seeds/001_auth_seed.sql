SET NOCOUNT ON;

DECLARE @Roles TABLE(RoleName NVARCHAR(100) NOT NULL PRIMARY KEY);
INSERT INTO @Roles(RoleName)
VALUES
('Admin'),
('Claim Manager'),
('Investigator'),
('Adjuster'),
('Finance'),
('Fraud Analyst');

MERGE dbo.Roles AS Target
USING @Roles AS Source
ON Target.Name = Source.RoleName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (RoleId, Name)
    VALUES (NEWID(), Source.RoleName);

DECLARE @Permissions TABLE(PermissionName NVARCHAR(150) NOT NULL PRIMARY KEY);
INSERT INTO @Permissions(PermissionName)
VALUES
('Users.Manage'),
('Claims.Read'),
('Claims.Assign'),
('Claims.Investigate'),
('Claims.Adjust'),
('Payments.Process'),
('Fraud.Review'),
('Reports.View');

MERGE dbo.Permissions AS Target
USING @Permissions AS Source
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @RolePermissionMap TABLE
(
    RoleName NVARCHAR(100) NOT NULL,
    PermissionName NVARCHAR(150) NOT NULL,
    PRIMARY KEY (RoleName, PermissionName)
);

INSERT INTO @RolePermissionMap(RoleName, PermissionName)
VALUES
('Admin', 'Users.Manage'),
('Admin', 'Claims.Read'),
('Admin', 'Claims.Assign'),
('Admin', 'Claims.Investigate'),
('Admin', 'Claims.Adjust'),
('Admin', 'Payments.Process'),
('Admin', 'Fraud.Review'),
('Admin', 'Reports.View'),
('Claim Manager', 'Claims.Read'),
('Claim Manager', 'Claims.Assign'),
('Claim Manager', 'Reports.View'),
('Investigator', 'Claims.Read'),
('Investigator', 'Claims.Investigate'),
('Adjuster', 'Claims.Read'),
('Adjuster', 'Claims.Adjust'),
('Finance', 'Claims.Read'),
('Finance', 'Payments.Process'),
('Fraud Analyst', 'Claims.Read'),
('Fraud Analyst', 'Fraud.Review');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
SELECT NEWID(), r.RoleId, p.PermissionId
FROM @RolePermissionMap rpm
INNER JOIN dbo.Roles r ON r.Name = rpm.RoleName
INNER JOIN dbo.Permissions p ON p.Name = rpm.PermissionName
LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = r.RoleId AND rp.PermissionId = p.PermissionId
WHERE rp.RolePermissionId IS NULL;

GO
