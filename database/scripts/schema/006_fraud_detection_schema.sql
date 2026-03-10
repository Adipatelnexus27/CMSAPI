IF OBJECT_ID('dbo.FraudFlags', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FraudFlags
    (
        FraudFlagId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        FlagType NVARCHAR(100) NOT NULL,
        RuleName NVARCHAR(200) NULL,
        SeverityScore INT NOT NULL,
        Reason NVARCHAR(1000) NOT NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_FraudFlags_Status DEFAULT('Open'),
        IsDuplicate BIT NOT NULL CONSTRAINT DF_FraudFlags_IsDuplicate DEFAULT(0),
        IsSuspicious BIT NOT NULL CONSTRAINT DF_FraudFlags_IsSuspicious DEFAULT(0),
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_FraudFlags_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        ReviewedByUserId UNIQUEIDENTIFIER NULL,
        ReviewedAtUtc DATETIME2 NULL,
        ReviewNote NVARCHAR(1000) NULL,
        CONSTRAINT FK_FraudFlags_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT FK_FraudFlags_CreatedByUser FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_FraudFlags_ReviewedByUser FOREIGN KEY (ReviewedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_FraudFlags_SeverityScore CHECK (SeverityScore >= 0 AND SeverityScore <= 100),
        CONSTRAINT CK_FraudFlags_Status CHECK (Status IN ('Open', 'UnderInvestigation', 'ConfirmedFraud', 'Cleared'))
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.FraudFlags')
      AND name = 'IX_FraudFlags_ClaimId_Status'
)
BEGIN
    CREATE INDEX IX_FraudFlags_ClaimId_Status ON dbo.FraudFlags(ClaimId, Status, CreatedAtUtc DESC);
END;
GO
