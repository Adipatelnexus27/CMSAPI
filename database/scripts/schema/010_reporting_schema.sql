IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Claims')
      AND name = 'IX_Claims_ClaimStatus_CreatedAtUtc'
)
AND OBJECT_ID('dbo.Claims', 'U') IS NOT NULL
BEGIN
    CREATE INDEX IX_Claims_ClaimStatus_CreatedAtUtc ON dbo.Claims(ClaimStatus, CreatedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.PolicyRegistry')
      AND name = 'IX_PolicyRegistry_ProductCode'
)
AND OBJECT_ID('dbo.PolicyRegistry', 'U') IS NOT NULL
BEGIN
    CREATE INDEX IX_PolicyRegistry_ProductCode ON dbo.PolicyRegistry(ProductCode);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.FraudFlags')
      AND name = 'IX_FraudFlags_Status_CreatedAtUtc'
)
AND OBJECT_ID('dbo.FraudFlags', 'U') IS NOT NULL
BEGIN
    CREATE INDEX IX_FraudFlags_Status_CreatedAtUtc ON dbo.FraudFlags(Status, CreatedAtUtc DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ClaimSettlements')
      AND name = 'IX_ClaimSettlements_CalculatedAtUtc'
)
AND OBJECT_ID('dbo.ClaimSettlements', 'U') IS NOT NULL
BEGIN
    CREATE INDEX IX_ClaimSettlements_CalculatedAtUtc ON dbo.ClaimSettlements(CalculatedAtUtc DESC);
END;
GO
