CREATE OR ALTER PROCEDURE dbo.sp_Fraud_CreateOrReuseFlag
    @ClaimId UNIQUEIDENTIFIER,
    @FlagType NVARCHAR(100),
    @RuleName NVARCHAR(200) = NULL,
    @SeverityScore INT,
    @Reason NVARCHAR(1000),
    @IsDuplicate BIT,
    @IsSuspicious BIT,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Claims WHERE ClaimId = @ClaimId)
    BEGIN
        THROW 56001, 'Claim not found.', 1;
    END;

    IF @SeverityScore < 0 OR @SeverityScore > 100
    BEGIN
        THROW 56002, 'Severity score must be between 0 and 100.', 1;
    END;

    DECLARE @ExistingFraudFlagId UNIQUEIDENTIFIER;

    SELECT TOP 1 @ExistingFraudFlagId = ff.FraudFlagId
    FROM dbo.FraudFlags ff
    WHERE ff.ClaimId = @ClaimId
      AND ff.FlagType = @FlagType
      AND ISNULL(ff.RuleName, '') = ISNULL(@RuleName, '')
      AND ff.Reason = @Reason
      AND ff.Status IN ('Open', 'UnderInvestigation', 'ConfirmedFraud')
    ORDER BY ff.CreatedAtUtc DESC;

    IF @ExistingFraudFlagId IS NULL
    BEGIN
        SET @ExistingFraudFlagId = NEWID();

        INSERT INTO dbo.FraudFlags
        (
            FraudFlagId,
            ClaimId,
            FlagType,
            RuleName,
            SeverityScore,
            Reason,
            Status,
            IsDuplicate,
            IsSuspicious,
            CreatedByUserId
        )
        VALUES
        (
            @ExistingFraudFlagId,
            @ClaimId,
            @FlagType,
            @RuleName,
            @SeverityScore,
            @Reason,
            'Open',
            @IsDuplicate,
            @IsSuspicious,
            @CreatedByUserId
        );

        INSERT INTO dbo.ClaimWorkflowHistory
        (
            ClaimWorkflowHistoryId,
            ClaimId,
            ActionType,
            PreviousValue,
            NewValue,
            ChangedByUserId,
            Notes
        )
        VALUES
        (
            NEWID(),
            @ClaimId,
            'FraudFlagCreated',
            NULL,
            @FlagType,
            @CreatedByUserId,
            LEFT(@Reason, 500)
        );
    END;

    SELECT
        ff.FraudFlagId,
        ff.ClaimId,
        c.ClaimNumber,
        ff.FlagType,
        ff.RuleName,
        ff.SeverityScore,
        ff.Reason,
        ff.Status,
        ff.IsDuplicate,
        ff.IsSuspicious,
        ff.CreatedAtUtc,
        ff.CreatedByUserId,
        ff.ReviewedByUserId,
        ff.ReviewedAtUtc,
        ff.ReviewNote
    FROM dbo.FraudFlags ff
    INNER JOIN dbo.Claims c ON c.ClaimId = ff.ClaimId
    WHERE ff.FraudFlagId = @ExistingFraudFlagId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Fraud_GetFlags
    @ClaimId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ff.FraudFlagId,
        ff.ClaimId,
        c.ClaimNumber,
        ff.FlagType,
        ff.RuleName,
        ff.SeverityScore,
        ff.Reason,
        ff.Status,
        ff.IsDuplicate,
        ff.IsSuspicious,
        ff.CreatedAtUtc,
        ff.CreatedByUserId,
        ff.ReviewedByUserId,
        ff.ReviewedAtUtc,
        ff.ReviewNote
    FROM dbo.FraudFlags ff
    INNER JOIN dbo.Claims c ON c.ClaimId = ff.ClaimId
    WHERE (@ClaimId IS NULL OR ff.ClaimId = @ClaimId)
      AND (@Status IS NULL OR ff.Status = @Status)
    ORDER BY ff.SeverityScore DESC, ff.CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Fraud_UpdateFlagStatus
    @FraudFlagId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ReviewNote NVARCHAR(1000) = NULL,
    @ReviewedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Status NOT IN ('Open', 'UnderInvestigation', 'ConfirmedFraud', 'Cleared')
    BEGIN
        THROW 56003, 'Invalid fraud flag status.', 1;
    END;

    DECLARE @ClaimId UNIQUEIDENTIFIER;
    DECLARE @PreviousStatus NVARCHAR(50);

    SELECT
        @ClaimId = ff.ClaimId,
        @PreviousStatus = ff.Status
    FROM dbo.FraudFlags ff
    WHERE ff.FraudFlagId = @FraudFlagId;

    IF @ClaimId IS NULL
    BEGIN
        THROW 56004, 'Fraud flag not found.', 1;
    END;

    UPDATE dbo.FraudFlags
    SET Status = @Status,
        ReviewNote = @ReviewNote,
        ReviewedByUserId = @ReviewedByUserId,
        ReviewedAtUtc = SYSUTCDATETIME()
    WHERE FraudFlagId = @FraudFlagId;

    INSERT INTO dbo.ClaimWorkflowHistory
    (
        ClaimWorkflowHistoryId,
        ClaimId,
        ActionType,
        PreviousValue,
        NewValue,
        ChangedByUserId,
        Notes
    )
    VALUES
    (
        NEWID(),
        @ClaimId,
        'FraudFlagStatusUpdated',
        @PreviousStatus,
        @Status,
        @ReviewedByUserId,
        LEFT(ISNULL(@ReviewNote, 'Fraud flag status updated'), 500)
    );
END;
GO
