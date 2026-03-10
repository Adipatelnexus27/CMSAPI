IF OBJECT_ID('dbo.ClaimReserves', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimReserves
    (
        ClaimReserveId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        CurrentReserveAmount DECIMAL(18,2) NOT NULL,
        CurrencyCode NVARCHAR(3) NOT NULL,
        LastApprovedAtUtc DATETIME2 NULL,
        LastApprovedByUserId UNIQUEIDENTIFIER NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimReserves_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimReserves_UpdatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_ClaimReserves_ClaimId UNIQUE (ClaimId),
        CONSTRAINT FK_ClaimReserves_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT FK_ClaimReserves_LastApprovedByUser FOREIGN KEY (LastApprovedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_ClaimReserves_CreatedByUser FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_ClaimReserves_Amount CHECK (CurrentReserveAmount > 0),
        CONSTRAINT CK_ClaimReserves_CurrencyCode CHECK (LEN(CurrencyCode) = 3)
    );
END;
GO

IF OBJECT_ID('dbo.ClaimReserveHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimReserveHistory
    (
        ClaimReserveHistoryId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimReserveId UNIQUEIDENTIFIER NOT NULL,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        ActionType NVARCHAR(50) NOT NULL,
        PreviousReserveAmount DECIMAL(18,2) NULL,
        RequestedReserveAmount DECIMAL(18,2) NOT NULL,
        ApprovedReserveAmount DECIMAL(18,2) NULL,
        CurrencyCode NVARCHAR(3) NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        Reason NVARCHAR(500) NULL,
        RequestedByUserId UNIQUEIDENTIFIER NULL,
        RequestedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimReserveHistory_RequestedAtUtc DEFAULT SYSUTCDATETIME(),
        ApprovedByUserId UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc DATETIME2 NULL,
        ApprovalNote NVARCHAR(1000) NULL,
        CONSTRAINT FK_ClaimReserveHistory_ClaimReserves FOREIGN KEY (ClaimReserveId) REFERENCES dbo.ClaimReserves(ClaimReserveId),
        CONSTRAINT FK_ClaimReserveHistory_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT FK_ClaimReserveHistory_RequestedByUser FOREIGN KEY (RequestedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_ClaimReserveHistory_ApprovedByUser FOREIGN KEY (ApprovedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_ClaimReserveHistory_RequestedReserveAmount CHECK (RequestedReserveAmount > 0),
        CONSTRAINT CK_ClaimReserveHistory_Status CHECK (Status IN ('PendingApproval', 'Approved', 'Rejected')),
        CONSTRAINT CK_ClaimReserveHistory_ActionType CHECK (ActionType IN ('InitialReserve', 'AdjustmentRequested', 'AdjustmentApproved', 'AdjustmentRejected')),
        CONSTRAINT CK_ClaimReserveHistory_CurrencyCode CHECK (LEN(CurrencyCode) = 3)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimReserveHistory')
      AND name = 'IX_ClaimReserveHistory_ClaimId_RequestedAtUtc'
)
BEGIN
    CREATE INDEX IX_ClaimReserveHistory_ClaimId_RequestedAtUtc ON dbo.ClaimReserveHistory(ClaimId, RequestedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimReserveHistory')
      AND name = 'IX_ClaimReserveHistory_Status_RequestedAtUtc'
)
BEGIN
    CREATE INDEX IX_ClaimReserveHistory_Status_RequestedAtUtc ON dbo.ClaimReserveHistory(Status, RequestedAtUtc DESC);
END;
GO
