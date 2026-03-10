CREATE OR ALTER PROCEDURE dbo.sp_Reports_ClaimsByStatus
    @FromDateUtc DATETIME2 = NULL,
    @ToDateUtc DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH FilteredClaims AS
    (
        SELECT c.ClaimStatus
        FROM dbo.Claims c
        WHERE (@FromDateUtc IS NULL OR c.CreatedAtUtc >= @FromDateUtc)
          AND (@ToDateUtc IS NULL OR c.CreatedAtUtc <= @ToDateUtc)
    )
    SELECT
        fc.ClaimStatus,
        COUNT(1) AS ClaimCount,
        CAST(COUNT(1) * 100.0 / NULLIF(SUM(COUNT(1)) OVER (), 0) AS DECIMAL(8,2)) AS PercentageOfTotal
    FROM FilteredClaims fc
    GROUP BY fc.ClaimStatus
    ORDER BY ClaimCount DESC, fc.ClaimStatus;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reports_ClaimsByProduct
    @FromDateUtc DATETIME2 = NULL,
    @ToDateUtc DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.ProductCode,
        COUNT(1) AS ClaimCount,
        SUM(CASE WHEN c.ClaimStatus IN ('New', 'Open', 'UnderInvestigation', 'Assigned', 'Processing') THEN 1 ELSE 0 END) AS OpenClaims,
        SUM(CASE WHEN c.ClaimStatus IN ('Closed', 'Settled', 'Rejected', 'Paid') THEN 1 ELSE 0 END) AS ClosedClaims
    FROM dbo.Claims c
    INNER JOIN dbo.PolicyRegistry p ON p.PolicyNumber = c.PolicyNumber
    WHERE (@FromDateUtc IS NULL OR c.CreatedAtUtc >= @FromDateUtc)
      AND (@ToDateUtc IS NULL OR c.CreatedAtUtc <= @ToDateUtc)
    GROUP BY p.ProductCode
    ORDER BY ClaimCount DESC, p.ProductCode;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reports_FraudSummary
    @FromDateUtc DATETIME2 = NULL,
    @ToDateUtc DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ff.Status AS FraudStatus,
        COUNT(1) AS FlagCount,
        SUM(CASE WHEN ff.IsDuplicate = 1 THEN 1 ELSE 0 END) AS DuplicateFlags,
        SUM(CASE WHEN ff.IsSuspicious = 1 THEN 1 ELSE 0 END) AS SuspiciousFlags,
        CAST(ISNULL(AVG(CAST(ff.SeverityScore AS DECIMAL(10,2))), 0) AS DECIMAL(10,2)) AS AverageSeverityScore
    FROM dbo.FraudFlags ff
    WHERE (@FromDateUtc IS NULL OR ff.CreatedAtUtc >= @FromDateUtc)
      AND (@ToDateUtc IS NULL OR ff.CreatedAtUtc <= @ToDateUtc)
    GROUP BY ff.Status
    ORDER BY FlagCount DESC, ff.Status;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reports_InvestigatorPerformance
    @FromDateUtc DATETIME2 = NULL,
    @ToDateUtc DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH FilteredClaims AS
    (
        SELECT
            c.ClaimId,
            c.InvestigatorUserId,
            c.ClaimStatus,
            c.InvestigationProgress
        FROM dbo.Claims c
        WHERE c.InvestigatorUserId IS NOT NULL
          AND (@FromDateUtc IS NULL OR c.CreatedAtUtc >= @FromDateUtc)
          AND (@ToDateUtc IS NULL OR c.CreatedAtUtc <= @ToDateUtc)
    ),
    NotesByInvestigator AS
    (
        SELECT
            fc.InvestigatorUserId,
            COUNT(1) AS TotalNotes
        FROM FilteredClaims fc
        INNER JOIN dbo.ClaimInvestigationNotes n
            ON n.ClaimId = fc.ClaimId
           AND n.CreatedByUserId = fc.InvestigatorUserId
        GROUP BY fc.InvestigatorUserId
    ),
    FraudFlagsByInvestigator AS
    (
        SELECT
            fc.InvestigatorUserId,
            COUNT(1) AS FraudFlagsOnAssignedClaims
        FROM FilteredClaims fc
        INNER JOIN dbo.FraudFlags ff
            ON ff.ClaimId = fc.ClaimId
        GROUP BY fc.InvestigatorUserId
    )
    SELECT
        fc.InvestigatorUserId,
        COALESCE(u.FullName, CONVERT(NVARCHAR(36), fc.InvestigatorUserId)) AS InvestigatorName,
        COUNT(1) AS AssignedClaims,
        SUM(CASE WHEN fc.ClaimStatus IN ('Closed', 'Settled', 'Rejected', 'Paid') THEN 1 ELSE 0 END) AS ClosedClaims,
        CAST(ISNULL(AVG(CAST(fc.InvestigationProgress AS DECIMAL(10,2))), 0) AS DECIMAL(10,2)) AS AverageInvestigationProgress,
        ISNULL(nbi.TotalNotes, 0) AS TotalNotes,
        ISNULL(fbi.FraudFlagsOnAssignedClaims, 0) AS FraudFlagsOnAssignedClaims
    FROM FilteredClaims fc
    LEFT JOIN dbo.Users u ON u.UserId = fc.InvestigatorUserId
    LEFT JOIN NotesByInvestigator nbi ON nbi.InvestigatorUserId = fc.InvestigatorUserId
    LEFT JOIN FraudFlagsByInvestigator fbi ON fbi.InvestigatorUserId = fc.InvestigatorUserId
    GROUP BY fc.InvestigatorUserId, u.FullName, nbi.TotalNotes, fbi.FraudFlagsOnAssignedClaims
    ORDER BY AssignedClaims DESC, InvestigatorName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reports_SettlementSummary
    @FromDateUtc DATETIME2 = NULL,
    @ToDateUtc DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cp.PaymentStatus,
        COUNT(1) AS PaymentCount,
        CAST(ISNULL(SUM(cp.PaymentAmount), 0) AS DECIMAL(18,2)) AS TotalPaymentAmount,
        CAST(ISNULL(AVG(cp.PaymentAmount), 0) AS DECIMAL(18,2)) AS AveragePaymentAmount
    FROM dbo.ClaimPayments cp
    WHERE (@FromDateUtc IS NULL OR cp.RequestedAtUtc >= @FromDateUtc)
      AND (@ToDateUtc IS NULL OR cp.RequestedAtUtc <= @ToDateUtc)
    GROUP BY cp.PaymentStatus
    ORDER BY PaymentCount DESC, cp.PaymentStatus;
END;
GO
