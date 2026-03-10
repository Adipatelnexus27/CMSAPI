IF COL_LENGTH('dbo.PolicyRegistry', 'PolicyLimitAmount') IS NULL
BEGIN
    ALTER TABLE dbo.PolicyRegistry
    ADD PolicyLimitAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PolicyRegistry_PolicyLimitAmount DEFAULT(100000.00);
END;
GO

IF COL_LENGTH('dbo.PolicyRegistry', 'DeductibleAmount') IS NULL
BEGIN
    ALTER TABLE dbo.PolicyRegistry
    ADD DeductibleAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PolicyRegistry_DeductibleAmount DEFAULT(500.00);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_PolicyRegistry_PolicyLimitAmount'
      AND parent_object_id = OBJECT_ID('dbo.PolicyRegistry')
)
BEGIN
    ALTER TABLE dbo.PolicyRegistry
    ADD CONSTRAINT CK_PolicyRegistry_PolicyLimitAmount CHECK (PolicyLimitAmount > 0);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_PolicyRegistry_DeductibleAmount'
      AND parent_object_id = OBJECT_ID('dbo.PolicyRegistry')
)
BEGIN
    ALTER TABLE dbo.PolicyRegistry
    ADD CONSTRAINT CK_PolicyRegistry_DeductibleAmount CHECK (DeductibleAmount >= 0);
END;
GO

IF OBJECT_ID('dbo.ClaimSettlements', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimSettlements
    (
        ClaimSettlementId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        GrossLossAmount DECIMAL(18,2) NOT NULL,
        PolicyLimitAmount DECIMAL(18,2) NOT NULL,
        DeductibleAmount DECIMAL(18,2) NOT NULL,
        EligibleAmount DECIMAL(18,2) NOT NULL,
        ApprovedSettlementAmount DECIMAL(18,2) NOT NULL,
        CurrencyCode NVARCHAR(3) NOT NULL,
        CalculationStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_ClaimSettlements_CalculationStatus DEFAULT('Calculated'),
        CalculatedByUserId UNIQUEIDENTIFIER NULL,
        CalculatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimSettlements_CalculatedAtUtc DEFAULT SYSUTCDATETIME(),
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimSettlements_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimSettlements_UpdatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_ClaimSettlements_ClaimId UNIQUE (ClaimId),
        CONSTRAINT FK_ClaimSettlements_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT FK_ClaimSettlements_CalculatedByUser FOREIGN KEY (CalculatedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_ClaimSettlements_GrossLossAmount CHECK (GrossLossAmount > 0),
        CONSTRAINT CK_ClaimSettlements_PolicyLimitAmount CHECK (PolicyLimitAmount > 0),
        CONSTRAINT CK_ClaimSettlements_DeductibleAmount CHECK (DeductibleAmount >= 0),
        CONSTRAINT CK_ClaimSettlements_EligibleAmount CHECK (EligibleAmount >= 0),
        CONSTRAINT CK_ClaimSettlements_ApprovedSettlementAmount CHECK (ApprovedSettlementAmount >= 0),
        CONSTRAINT CK_ClaimSettlements_CurrencyCode CHECK (LEN(CurrencyCode) = 3)
    );
END;
GO

IF OBJECT_ID('dbo.ClaimPayments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimPayments
    (
        ClaimPaymentId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimSettlementId UNIQUEIDENTIFIER NOT NULL,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        PaymentAmount DECIMAL(18,2) NOT NULL,
        CurrencyCode NVARCHAR(3) NOT NULL,
        PaymentStatus NVARCHAR(50) NOT NULL,
        RequestNote NVARCHAR(1000) NULL,
        RequestedByUserId UNIQUEIDENTIFIER NULL,
        RequestedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimPayments_RequestedAtUtc DEFAULT SYSUTCDATETIME(),
        ApprovedByUserId UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc DATETIME2 NULL,
        ApprovalNote NVARCHAR(1000) NULL,
        StatusNote NVARCHAR(1000) NULL,
        LastStatusUpdatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimPayments_LastStatusUpdatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ClaimPayments_ClaimSettlements FOREIGN KEY (ClaimSettlementId) REFERENCES dbo.ClaimSettlements(ClaimSettlementId),
        CONSTRAINT FK_ClaimPayments_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT FK_ClaimPayments_RequestedByUser FOREIGN KEY (RequestedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_ClaimPayments_ApprovedByUser FOREIGN KEY (ApprovedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_ClaimPayments_Amount CHECK (PaymentAmount > 0),
        CONSTRAINT CK_ClaimPayments_Status CHECK (PaymentStatus IN ('PendingApproval', 'Approved', 'Rejected', 'Processing', 'Paid', 'Failed')),
        CONSTRAINT CK_ClaimPayments_CurrencyCode CHECK (LEN(CurrencyCode) = 3)
    );
END;
GO

IF OBJECT_ID('dbo.ClaimPaymentStatusHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimPaymentStatusHistory
    (
        ClaimPaymentStatusHistoryId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimPaymentId UNIQUEIDENTIFIER NOT NULL,
        PreviousStatus NVARCHAR(50) NULL,
        NewStatus NVARCHAR(50) NOT NULL,
        Note NVARCHAR(1000) NULL,
        ChangedByUserId UNIQUEIDENTIFIER NULL,
        ChangedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimPaymentStatusHistory_ChangedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ClaimPaymentStatusHistory_ClaimPayments FOREIGN KEY (ClaimPaymentId) REFERENCES dbo.ClaimPayments(ClaimPaymentId),
        CONSTRAINT FK_ClaimPaymentStatusHistory_ChangedByUser FOREIGN KEY (ChangedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_ClaimPaymentStatusHistory_NewStatus CHECK (NewStatus IN ('PendingApproval', 'Approved', 'Rejected', 'Processing', 'Paid', 'Failed'))
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimPayments')
      AND name = 'IX_ClaimPayments_ClaimId_PaymentStatus'
)
BEGIN
    CREATE INDEX IX_ClaimPayments_ClaimId_PaymentStatus ON dbo.ClaimPayments(ClaimId, PaymentStatus, RequestedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimPayments')
      AND name = 'IX_ClaimPayments_PaymentStatus_RequestedAtUtc'
)
BEGIN
    CREATE INDEX IX_ClaimPayments_PaymentStatus_RequestedAtUtc ON dbo.ClaimPayments(PaymentStatus, RequestedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimPaymentStatusHistory')
      AND name = 'IX_ClaimPaymentStatusHistory_ClaimPaymentId_ChangedAtUtc'
)
BEGIN
    CREATE INDEX IX_ClaimPaymentStatusHistory_ClaimPaymentId_ChangedAtUtc ON dbo.ClaimPaymentStatusHistory(ClaimPaymentId, ChangedAtUtc DESC);
END;
GO
