SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.ClaimDocuments', 'DocumentCategory') IS NULL
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD DocumentCategory NVARCHAR(50) NOT NULL CONSTRAINT DF_ClaimDocuments_DocumentCategory DEFAULT('General');
END;
GO

IF COL_LENGTH('dbo.ClaimDocuments', 'DocumentGroupId') IS NULL
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD DocumentGroupId UNIQUEIDENTIFIER NULL;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ClaimDocuments')
      AND name = 'DocumentGroupId'
      AND is_nullable = 1
)
BEGIN
    UPDATE dbo.ClaimDocuments
    SET DocumentGroupId = ClaimDocumentId
    WHERE DocumentGroupId IS NULL;

    ALTER TABLE dbo.ClaimDocuments
    ALTER COLUMN DocumentGroupId UNIQUEIDENTIFIER NOT NULL;
END;
GO

IF COL_LENGTH('dbo.ClaimDocuments', 'VersionNumber') IS NULL
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD VersionNumber INT NOT NULL CONSTRAINT DF_ClaimDocuments_VersionNumber DEFAULT(1);
END;
GO

IF COL_LENGTH('dbo.ClaimDocuments', 'IsLatest') IS NULL
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD IsLatest BIT NOT NULL CONSTRAINT DF_ClaimDocuments_IsLatest DEFAULT(1);
END;
GO

IF COL_LENGTH('dbo.ClaimDocuments', 'UploadedByUserId') IS NULL
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD UploadedByUserId UNIQUEIDENTIFIER NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_ClaimDocuments_VersionNumber'
      AND parent_object_id = OBJECT_ID('dbo.ClaimDocuments')
)
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD CONSTRAINT CK_ClaimDocuments_VersionNumber CHECK (VersionNumber >= 1);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_ClaimDocuments_Users'
      AND parent_object_id = OBJECT_ID('dbo.ClaimDocuments')
)
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD CONSTRAINT FK_ClaimDocuments_Users FOREIGN KEY (UploadedByUserId) REFERENCES dbo.Users(UserId);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimDocuments')
      AND name = 'UX_ClaimDocuments_Group_Version'
)
BEGIN
    CREATE UNIQUE INDEX UX_ClaimDocuments_Group_Version
    ON dbo.ClaimDocuments(DocumentGroupId, VersionNumber);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimDocuments')
      AND name = 'UX_ClaimDocuments_Group_Latest'
)
BEGIN
    CREATE UNIQUE INDEX UX_ClaimDocuments_Group_Latest
    ON dbo.ClaimDocuments(DocumentGroupId)
    WHERE IsLatest = 1;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimDocuments')
      AND name = 'IX_ClaimDocuments_ClaimId_IsLatest'
)
BEGIN
    CREATE INDEX IX_ClaimDocuments_ClaimId_IsLatest
    ON dbo.ClaimDocuments(ClaimId, IsLatest, UploadedAtUtc DESC);
END;
GO
