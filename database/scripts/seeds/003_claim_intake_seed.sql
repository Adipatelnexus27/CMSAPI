SET NOCOUNT ON;

MERGE dbo.Permissions AS Target
USING (VALUES ('Claims.Create'), ('Claims.DocumentsUpload'), ('Claims.Link')) AS Source(PermissionName)
ON Target.Name = Source.PermissionName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PermissionId, Name)
    VALUES (NEWID(), Source.PermissionName);

DECLARE @ClaimsCreatePermissionId UNIQUEIDENTIFIER;
DECLARE @ClaimsUploadPermissionId UNIQUEIDENTIFIER;
DECLARE @ClaimsLinkPermissionId UNIQUEIDENTIFIER;
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER;
DECLARE @AdjusterRoleId UNIQUEIDENTIFIER;
DECLARE @InvestigatorRoleId UNIQUEIDENTIFIER;

SELECT @ClaimsCreatePermissionId = PermissionId FROM dbo.Permissions WHERE Name = 'Claims.Create';
SELECT @ClaimsUploadPermissionId = PermissionId FROM dbo.Permissions WHERE Name = 'Claims.DocumentsUpload';
SELECT @ClaimsLinkPermissionId = PermissionId FROM dbo.Permissions WHERE Name = 'Claims.Link';
SELECT @AdminRoleId = RoleId FROM dbo.Roles WHERE Name = 'Admin';
SELECT @ClaimManagerRoleId = RoleId FROM dbo.Roles WHERE Name = 'Claim Manager';
SELECT @AdjusterRoleId = RoleId FROM dbo.Roles WHERE Name = 'Adjuster';
SELECT @InvestigatorRoleId = RoleId FROM dbo.Roles WHERE Name = 'Investigator';

IF @AdminRoleId IS NOT NULL AND @ClaimsCreatePermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @ClaimsCreatePermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @AdminRoleId, @ClaimsCreatePermissionId);
END;

IF @AdminRoleId IS NOT NULL AND @ClaimsUploadPermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @ClaimsUploadPermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @AdminRoleId, @ClaimsUploadPermissionId);
END;

IF @AdminRoleId IS NOT NULL AND @ClaimsLinkPermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @ClaimsLinkPermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @AdminRoleId, @ClaimsLinkPermissionId);
END;

IF @ClaimManagerRoleId IS NOT NULL AND @ClaimsCreatePermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @ClaimManagerRoleId AND PermissionId = @ClaimsCreatePermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @ClaimManagerRoleId, @ClaimsCreatePermissionId);
END;

IF @ClaimManagerRoleId IS NOT NULL AND @ClaimsUploadPermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @ClaimManagerRoleId AND PermissionId = @ClaimsUploadPermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @ClaimManagerRoleId, @ClaimsUploadPermissionId);
END;

IF @ClaimManagerRoleId IS NOT NULL AND @ClaimsLinkPermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @ClaimManagerRoleId AND PermissionId = @ClaimsLinkPermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @ClaimManagerRoleId, @ClaimsLinkPermissionId);
END;

IF @AdjusterRoleId IS NOT NULL AND @ClaimsCreatePermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @AdjusterRoleId AND PermissionId = @ClaimsCreatePermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @AdjusterRoleId, @ClaimsCreatePermissionId);
END;

IF @AdjusterRoleId IS NOT NULL AND @ClaimsUploadPermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @AdjusterRoleId AND PermissionId = @ClaimsUploadPermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @AdjusterRoleId, @ClaimsUploadPermissionId);
END;

IF @InvestigatorRoleId IS NOT NULL AND @ClaimsLinkPermissionId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @InvestigatorRoleId AND PermissionId = @ClaimsLinkPermissionId)
BEGIN
    INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
    VALUES (NEWID(), @InvestigatorRoleId, @ClaimsLinkPermissionId);
END;

DECLARE @Policies TABLE
(
    PolicyNumber NVARCHAR(50) NOT NULL PRIMARY KEY,
    PolicyHolderName NVARCHAR(200) NOT NULL,
    ProductCode NVARCHAR(100) NOT NULL,
    PolicyTypeCode NVARCHAR(100) NOT NULL,
    EffectiveDateUtc DATETIME2 NOT NULL,
    ExpiryDateUtc DATETIME2 NOT NULL,
    IsActive BIT NOT NULL
);

INSERT INTO @Policies(PolicyNumber, PolicyHolderName, ProductCode, PolicyTypeCode, EffectiveDateUtc, ExpiryDateUtc, IsActive)
VALUES
('POL-2026-0001', 'Alice Johnson', 'MTPL', 'ANNUAL', '2026-01-01', '2026-12-31', 1),
('POL-2026-0002', 'Robert Singh', 'CASCO', 'ANNUAL', '2026-01-01', '2026-12-31', 1),
('POL-2026-0003', 'Marta Lewis', 'MTPL', 'MONTHLY', '2026-02-01', '2026-03-01', 1);

MERGE dbo.PolicyRegistry AS Target
USING @Policies AS Source
ON Target.PolicyNumber = Source.PolicyNumber
WHEN MATCHED THEN
    UPDATE SET
        PolicyHolderName = Source.PolicyHolderName,
        ProductCode = Source.ProductCode,
        PolicyTypeCode = Source.PolicyTypeCode,
        EffectiveDateUtc = Source.EffectiveDateUtc,
        ExpiryDateUtc = Source.ExpiryDateUtc,
        IsActive = Source.IsActive
WHEN NOT MATCHED BY TARGET THEN
    INSERT
    (
        PolicyNumber,
        PolicyHolderName,
        ProductCode,
        PolicyTypeCode,
        EffectiveDateUtc,
        ExpiryDateUtc,
        IsActive
    )
    VALUES
    (
        Source.PolicyNumber,
        Source.PolicyHolderName,
        Source.ProductCode,
        Source.PolicyTypeCode,
        Source.EffectiveDateUtc,
        Source.ExpiryDateUtc,
        Source.IsActive
    );
GO
