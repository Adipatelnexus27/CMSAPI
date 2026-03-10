IF COL_LENGTH('dbo.Claims', 'InvestigationProgress') IS NULL
BEGIN
    ALTER TABLE dbo.Claims
    ADD InvestigationProgress INT NOT NULL CONSTRAINT DF_Claims_InvestigationProgress DEFAULT(0);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Claims_InvestigationProgress'
      AND parent_object_id = OBJECT_ID('dbo.Claims')
)
BEGIN
    ALTER TABLE dbo.Claims WITH NOCHECK
    ADD CONSTRAINT CK_Claims_InvestigationProgress CHECK (InvestigationProgress >= 0 AND InvestigationProgress <= 100);
END;
GO

IF COL_LENGTH('dbo.ClaimDocuments', 'DocumentCategory') IS NULL
BEGIN
    ALTER TABLE dbo.ClaimDocuments
    ADD DocumentCategory NVARCHAR(50) NOT NULL CONSTRAINT DF_ClaimDocuments_DocumentCategory DEFAULT('General');
END;
GO

IF OBJECT_ID('dbo.ClaimInvestigationNotes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimInvestigationNotes
    (
        ClaimInvestigationNoteId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        NoteText NVARCHAR(MAX) NOT NULL,
        ProgressPercentSnapshot INT NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimInvestigationNotes_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ClaimInvestigationNotes_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT FK_ClaimInvestigationNotes_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimInvestigationNotes')
      AND name = 'IX_ClaimInvestigationNotes_ClaimId'
)
BEGIN
    CREATE INDEX IX_ClaimInvestigationNotes_ClaimId ON dbo.ClaimInvestigationNotes(ClaimId, CreatedAtUtc DESC);
END;
GO
