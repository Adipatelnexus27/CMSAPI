IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        AuditLogId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EventType NVARCHAR(50) NOT NULL,
        ActionName NVARCHAR(200) NOT NULL,
        EntityName NVARCHAR(100) NULL,
        EntityId UNIQUEIDENTIFIER NULL,
        ClaimId UNIQUEIDENTIFIER NULL,
        Description NVARCHAR(2000) NULL,
        RequestMethod NVARCHAR(10) NULL,
        RequestPath NVARCHAR(500) NULL,
        RequestQuery NVARCHAR(1000) NULL,
        HttpStatusCode INT NULL,
        IsSuccess BIT NOT NULL CONSTRAINT DF_AuditLogs_IsSuccess DEFAULT (1),
        DurationMs INT NULL,
        UserId UNIQUEIDENTIFIER NULL,
        UserEmail NVARCHAR(320) NULL,
        UserRoleCsv NVARCHAR(500) NULL,
        IpAddress NVARCHAR(100) NULL,
        UserAgent NVARCHAR(500) NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_AuditLogs_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.AuditLogs')
      AND name = 'IX_AuditLogs_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_AuditLogs_CreatedAtUtc
        ON dbo.AuditLogs(CreatedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.AuditLogs')
      AND name = 'IX_AuditLogs_EventType_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_AuditLogs_EventType_CreatedAtUtc
        ON dbo.AuditLogs(EventType, CreatedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.AuditLogs')
      AND name = 'IX_AuditLogs_ClaimId_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_AuditLogs_ClaimId_CreatedAtUtc
        ON dbo.AuditLogs(ClaimId, CreatedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.AuditLogs')
      AND name = 'IX_AuditLogs_UserId_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_AuditLogs_UserId_CreatedAtUtc
        ON dbo.AuditLogs(UserId, CreatedAtUtc DESC);
END;
GO
