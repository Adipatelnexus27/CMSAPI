IF OBJECT_ID('dbo.PolicyRegistry', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PolicyRegistry
    (
        PolicyNumber NVARCHAR(50) NOT NULL PRIMARY KEY,
        PolicyHolderName NVARCHAR(200) NOT NULL,
        ProductCode NVARCHAR(100) NOT NULL,
        PolicyTypeCode NVARCHAR(100) NOT NULL,
        EffectiveDateUtc DATETIME2 NOT NULL,
        ExpiryDateUtc DATETIME2 NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_PolicyRegistry_IsActive DEFAULT(1),
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_PolicyRegistry_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID('dbo.Claims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Claims
    (
        ClaimId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimNumber NVARCHAR(50) NOT NULL,
        PolicyNumber NVARCHAR(50) NOT NULL,
        ClaimType NVARCHAR(100) NOT NULL,
        ClaimStatus NVARCHAR(100) NOT NULL,
        ReporterName NVARCHAR(200) NOT NULL,
        IncidentDateUtc DATETIME2 NOT NULL,
        IncidentLocation NVARCHAR(300) NOT NULL,
        IncidentDescription NVARCHAR(MAX) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Claims_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        CONSTRAINT FK_Claims_PolicyRegistry FOREIGN KEY (PolicyNumber) REFERENCES dbo.PolicyRegistry(PolicyNumber)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Claims') AND name = 'UQ_Claims_ClaimNumber')
BEGIN
    CREATE UNIQUE INDEX UQ_Claims_ClaimNumber ON dbo.Claims(ClaimNumber);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Claims') AND name = 'IX_Claims_PolicyNumber')
BEGIN
    CREATE INDEX IX_Claims_PolicyNumber ON dbo.Claims(PolicyNumber);
END;
GO

IF OBJECT_ID('dbo.ClaimDocuments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimDocuments
    (
        ClaimDocumentId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        OriginalFileName NVARCHAR(300) NOT NULL,
        StoredFilePath NVARCHAR(1000) NOT NULL,
        ContentType NVARCHAR(100) NOT NULL,
        FileSizeBytes BIGINT NOT NULL,
        UploadedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimDocuments_UploadedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ClaimDocuments_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.ClaimDocuments') AND name = 'IX_ClaimDocuments_ClaimId')
BEGIN
    CREATE INDEX IX_ClaimDocuments_ClaimId ON dbo.ClaimDocuments(ClaimId);
END;
GO

IF OBJECT_ID('dbo.ClaimRelations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimRelations
    (
        ClaimRelationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        RelatedClaimId UNIQUEIDENTIFIER NOT NULL,
        LinkedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimRelations_LinkedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ClaimRelations_Claims_ClaimId FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT FK_ClaimRelations_Claims_RelatedClaimId FOREIGN KEY (RelatedClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT CK_ClaimRelations_NotSelf CHECK (ClaimId <> RelatedClaimId),
        CONSTRAINT UQ_ClaimRelations UNIQUE (ClaimId, RelatedClaimId)
    );
END;
GO

IF OBJECT_ID('dbo.Seq_ClaimNumber', 'SO') IS NULL
BEGIN
    EXEC ('CREATE SEQUENCE dbo.Seq_ClaimNumber AS BIGINT START WITH 1 INCREMENT BY 1');
END;
GO
