CREATE OR ALTER PROCEDURE dbo.sp_Settlement_Calculate
    @ClaimId UNIQUEIDENTIFIER,
    @GrossLossAmount DECIMAL(18,2),
    @CurrencyCode NVARCHAR(3),
    @CalculatedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @GrossLossAmount <= 0
    BEGIN
        THROW 58001, 'Gross loss amount must be greater than 0.', 1;
    END;

    IF LEN(ISNULL(@CurrencyCode, '')) <> 3
    BEGIN
        THROW 58002, 'Currency code must be a 3-letter code.', 1;
    END;

    DECLARE @ClaimNumber NVARCHAR(50);
    DECLARE @PolicyNumber NVARCHAR(50);
    DECLARE @PolicyLimitAmount DECIMAL(18,2);
    DECLARE @DeductibleAmount DECIMAL(18,2);

    SELECT
        @ClaimNumber = c.ClaimNumber,
        @PolicyNumber = c.PolicyNumber
    FROM dbo.Claims c
    WHERE c.ClaimId = @ClaimId;

    IF @ClaimNumber IS NULL
    BEGIN
        THROW 58003, 'Claim not found.', 1;
    END;

    SELECT
        @PolicyLimitAmount = p.PolicyLimitAmount,
        @DeductibleAmount = p.DeductibleAmount
    FROM dbo.PolicyRegistry p
    WHERE p.PolicyNumber = @PolicyNumber;

    IF @PolicyLimitAmount IS NULL
    BEGIN
        THROW 58004, 'Policy data not found for settlement calculation.', 1;
    END;

    DECLARE @EligibleAmount DECIMAL(18,2) = @GrossLossAmount - @DeductibleAmount;
    IF @EligibleAmount < 0 SET @EligibleAmount = 0;

    DECLARE @ApprovedSettlementAmount DECIMAL(18,2) = @EligibleAmount;
    IF @ApprovedSettlementAmount > @PolicyLimitAmount SET @ApprovedSettlementAmount = @PolicyLimitAmount;

    DECLARE @ClaimSettlementId UNIQUEIDENTIFIER;
    DECLARE @PreviousApprovedAmount DECIMAL(18,2);

    SELECT
        @ClaimSettlementId = s.ClaimSettlementId,
        @PreviousApprovedAmount = s.ApprovedSettlementAmount
    FROM dbo.ClaimSettlements s
    WHERE s.ClaimId = @ClaimId;

    IF @ClaimSettlementId IS NULL
    BEGIN
        SET @ClaimSettlementId = NEWID();

        INSERT INTO dbo.ClaimSettlements
        (
            ClaimSettlementId,
            ClaimId,
            GrossLossAmount,
            PolicyLimitAmount,
            DeductibleAmount,
            EligibleAmount,
            ApprovedSettlementAmount,
            CurrencyCode,
            CalculationStatus,
            CalculatedByUserId,
            CalculatedAtUtc,
            UpdatedAtUtc
        )
        VALUES
        (
            @ClaimSettlementId,
            @ClaimId,
            @GrossLossAmount,
            @PolicyLimitAmount,
            @DeductibleAmount,
            @EligibleAmount,
            @ApprovedSettlementAmount,
            UPPER(@CurrencyCode),
            'Calculated',
            @CalculatedByUserId,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
    END
    ELSE
    BEGIN
        UPDATE dbo.ClaimSettlements
        SET GrossLossAmount = @GrossLossAmount,
            PolicyLimitAmount = @PolicyLimitAmount,
            DeductibleAmount = @DeductibleAmount,
            EligibleAmount = @EligibleAmount,
            ApprovedSettlementAmount = @ApprovedSettlementAmount,
            CurrencyCode = UPPER(@CurrencyCode),
            CalculationStatus = 'Calculated',
            CalculatedByUserId = @CalculatedByUserId,
            CalculatedAtUtc = SYSUTCDATETIME(),
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE ClaimSettlementId = @ClaimSettlementId;
    END;

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
        'SettlementCalculated',
        CASE WHEN @PreviousApprovedAmount IS NULL THEN NULL ELSE CONVERT(NVARCHAR(300), @PreviousApprovedAmount) END,
        CONVERT(NVARCHAR(300), @ApprovedSettlementAmount),
        @CalculatedByUserId,
        CONCAT('GrossLoss=', CONVERT(NVARCHAR(100), @GrossLossAmount), '; Deductible=', CONVERT(NVARCHAR(100), @DeductibleAmount), '; PolicyLimit=', CONVERT(NVARCHAR(100), @PolicyLimitAmount))
    );

    SELECT
        s.ClaimSettlementId,
        s.ClaimId,
        c.ClaimNumber,
        s.GrossLossAmount,
        s.PolicyLimitAmount,
        s.DeductibleAmount,
        s.EligibleAmount,
        s.ApprovedSettlementAmount,
        s.CurrencyCode,
        s.CalculationStatus,
        s.CalculatedByUserId,
        s.CalculatedAtUtc,
        s.UpdatedAtUtc
    FROM dbo.ClaimSettlements s
    INNER JOIN dbo.Claims c ON c.ClaimId = s.ClaimId
    WHERE s.ClaimSettlementId = @ClaimSettlementId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Settlement_GetByClaimId
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        s.ClaimSettlementId,
        s.ClaimId,
        c.ClaimNumber,
        s.GrossLossAmount,
        s.PolicyLimitAmount,
        s.DeductibleAmount,
        s.EligibleAmount,
        s.ApprovedSettlementAmount,
        s.CurrencyCode,
        s.CalculationStatus,
        s.CalculatedByUserId,
        s.CalculatedAtUtc,
        s.UpdatedAtUtc
    FROM dbo.ClaimSettlements s
    INNER JOIN dbo.Claims c ON c.ClaimId = s.ClaimId
    WHERE s.ClaimId = @ClaimId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Settlement_RequestPayment
    @ClaimSettlementId UNIQUEIDENTIFIER,
    @PaymentAmount DECIMAL(18,2),
    @RequestNote NVARCHAR(1000) = NULL,
    @RequestedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @PaymentAmount <= 0
    BEGIN
        THROW 58005, 'Payment amount must be greater than 0.', 1;
    END;

    DECLARE @ClaimId UNIQUEIDENTIFIER;
    DECLARE @ApprovedSettlementAmount DECIMAL(18,2);
    DECLARE @CurrencyCode NVARCHAR(3);

    SELECT
        @ClaimId = s.ClaimId,
        @ApprovedSettlementAmount = s.ApprovedSettlementAmount,
        @CurrencyCode = s.CurrencyCode
    FROM dbo.ClaimSettlements s
    WHERE s.ClaimSettlementId = @ClaimSettlementId;

    IF @ClaimId IS NULL
    BEGIN
        THROW 58006, 'Claim settlement not found.', 1;
    END;

    DECLARE @CommittedAmount DECIMAL(18,2);
    SELECT @CommittedAmount = ISNULL(SUM(p.PaymentAmount), 0)
    FROM dbo.ClaimPayments p
    WHERE p.ClaimSettlementId = @ClaimSettlementId
      AND p.PaymentStatus IN ('PendingApproval', 'Approved', 'Processing', 'Paid');

    IF @CommittedAmount + @PaymentAmount > @ApprovedSettlementAmount
    BEGIN
        THROW 58007, 'Requested payment exceeds approved settlement amount.', 1;
    END;

    DECLARE @ClaimPaymentId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.ClaimPayments
    (
        ClaimPaymentId,
        ClaimSettlementId,
        ClaimId,
        PaymentAmount,
        CurrencyCode,
        PaymentStatus,
        RequestNote,
        RequestedByUserId,
        RequestedAtUtc,
        LastStatusUpdatedAtUtc
    )
    VALUES
    (
        @ClaimPaymentId,
        @ClaimSettlementId,
        @ClaimId,
        @PaymentAmount,
        @CurrencyCode,
        'PendingApproval',
        @RequestNote,
        @RequestedByUserId,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );

    INSERT INTO dbo.ClaimPaymentStatusHistory
    (
        ClaimPaymentStatusHistoryId,
        ClaimPaymentId,
        PreviousStatus,
        NewStatus,
        Note,
        ChangedByUserId,
        ChangedAtUtc
    )
    VALUES
    (
        NEWID(),
        @ClaimPaymentId,
        NULL,
        'PendingApproval',
        @RequestNote,
        @RequestedByUserId,
        SYSUTCDATETIME()
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
        'PaymentApprovalRequested',
        NULL,
        CONVERT(NVARCHAR(300), @PaymentAmount),
        @RequestedByUserId,
        LEFT(ISNULL(@RequestNote, 'Payment approval requested'), 500)
    );

    SELECT
        p.ClaimPaymentId,
        p.ClaimSettlementId,
        p.ClaimId,
        c.ClaimNumber,
        p.PaymentAmount,
        p.CurrencyCode,
        p.PaymentStatus,
        p.RequestNote,
        p.RequestedByUserId,
        p.RequestedAtUtc,
        p.ApprovedByUserId,
        p.ApprovedAtUtc,
        p.ApprovalNote,
        p.StatusNote,
        p.LastStatusUpdatedAtUtc
    FROM dbo.ClaimPayments p
    INNER JOIN dbo.Claims c ON c.ClaimId = p.ClaimId
    WHERE p.ClaimPaymentId = @ClaimPaymentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Settlement_GetPayments
    @ClaimId UNIQUEIDENTIFIER = NULL,
    @PaymentStatus NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.ClaimPaymentId,
        p.ClaimSettlementId,
        p.ClaimId,
        c.ClaimNumber,
        p.PaymentAmount,
        p.CurrencyCode,
        p.PaymentStatus,
        p.RequestNote,
        p.RequestedByUserId,
        p.RequestedAtUtc,
        p.ApprovedByUserId,
        p.ApprovedAtUtc,
        p.ApprovalNote,
        p.StatusNote,
        p.LastStatusUpdatedAtUtc
    FROM dbo.ClaimPayments p
    INNER JOIN dbo.Claims c ON c.ClaimId = p.ClaimId
    WHERE (@ClaimId IS NULL OR p.ClaimId = @ClaimId)
      AND (@PaymentStatus IS NULL OR p.PaymentStatus = @PaymentStatus)
    ORDER BY p.RequestedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Settlement_ApprovePayment
    @ClaimPaymentId UNIQUEIDENTIFIER,
    @ApprovalNote NVARCHAR(1000) = NULL,
    @ApprovedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ClaimId UNIQUEIDENTIFIER;
    DECLARE @PreviousStatus NVARCHAR(50);
    DECLARE @PaymentAmount DECIMAL(18,2);

    SELECT
        @ClaimId = p.ClaimId,
        @PreviousStatus = p.PaymentStatus,
        @PaymentAmount = p.PaymentAmount
    FROM dbo.ClaimPayments p
    WHERE p.ClaimPaymentId = @ClaimPaymentId;

    IF @ClaimId IS NULL
    BEGIN
        THROW 58008, 'Claim payment not found.', 1;
    END;

    IF @PreviousStatus <> 'PendingApproval'
    BEGIN
        THROW 58009, 'Only pending payments can be approved.', 1;
    END;

    UPDATE dbo.ClaimPayments
    SET PaymentStatus = 'Approved',
        ApprovedByUserId = @ApprovedByUserId,
        ApprovedAtUtc = SYSUTCDATETIME(),
        ApprovalNote = @ApprovalNote,
        LastStatusUpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimPaymentId = @ClaimPaymentId;

    INSERT INTO dbo.ClaimPaymentStatusHistory
    (
        ClaimPaymentStatusHistoryId,
        ClaimPaymentId,
        PreviousStatus,
        NewStatus,
        Note,
        ChangedByUserId,
        ChangedAtUtc
    )
    VALUES
    (
        NEWID(),
        @ClaimPaymentId,
        @PreviousStatus,
        'Approved',
        @ApprovalNote,
        @ApprovedByUserId,
        SYSUTCDATETIME()
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
        'PaymentApproved',
        @PreviousStatus,
        'Approved',
        @ApprovedByUserId,
        CONCAT('Amount=', CONVERT(NVARCHAR(100), @PaymentAmount), '; Note=', ISNULL(@ApprovalNote, 'Payment approved'))
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Settlement_RejectPayment
    @ClaimPaymentId UNIQUEIDENTIFIER,
    @ApprovalNote NVARCHAR(1000) = NULL,
    @ApprovedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ClaimId UNIQUEIDENTIFIER;
    DECLARE @PreviousStatus NVARCHAR(50);
    DECLARE @PaymentAmount DECIMAL(18,2);

    SELECT
        @ClaimId = p.ClaimId,
        @PreviousStatus = p.PaymentStatus,
        @PaymentAmount = p.PaymentAmount
    FROM dbo.ClaimPayments p
    WHERE p.ClaimPaymentId = @ClaimPaymentId;

    IF @ClaimId IS NULL
    BEGIN
        THROW 58008, 'Claim payment not found.', 1;
    END;

    IF @PreviousStatus <> 'PendingApproval'
    BEGIN
        THROW 58010, 'Only pending payments can be rejected.', 1;
    END;

    UPDATE dbo.ClaimPayments
    SET PaymentStatus = 'Rejected',
        ApprovedByUserId = @ApprovedByUserId,
        ApprovedAtUtc = SYSUTCDATETIME(),
        ApprovalNote = @ApprovalNote,
        LastStatusUpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimPaymentId = @ClaimPaymentId;

    INSERT INTO dbo.ClaimPaymentStatusHistory
    (
        ClaimPaymentStatusHistoryId,
        ClaimPaymentId,
        PreviousStatus,
        NewStatus,
        Note,
        ChangedByUserId,
        ChangedAtUtc
    )
    VALUES
    (
        NEWID(),
        @ClaimPaymentId,
        @PreviousStatus,
        'Rejected',
        @ApprovalNote,
        @ApprovedByUserId,
        SYSUTCDATETIME()
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
        'PaymentRejected',
        @PreviousStatus,
        'Rejected',
        @ApprovedByUserId,
        CONCAT('Amount=', CONVERT(NVARCHAR(100), @PaymentAmount), '; Note=', ISNULL(@ApprovalNote, 'Payment rejected'))
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Settlement_UpdatePaymentStatus
    @ClaimPaymentId UNIQUEIDENTIFIER,
    @PaymentStatus NVARCHAR(50),
    @StatusNote NVARCHAR(1000) = NULL,
    @ChangedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @PaymentStatus NOT IN ('Processing', 'Paid', 'Failed')
    BEGIN
        THROW 58011, 'Payment status must be one of: Processing, Paid, Failed.', 1;
    END;

    DECLARE @ClaimId UNIQUEIDENTIFIER;
    DECLARE @PreviousStatus NVARCHAR(50);

    SELECT
        @ClaimId = p.ClaimId,
        @PreviousStatus = p.PaymentStatus
    FROM dbo.ClaimPayments p
    WHERE p.ClaimPaymentId = @ClaimPaymentId;

    IF @ClaimId IS NULL
    BEGIN
        THROW 58008, 'Claim payment not found.', 1;
    END;

    IF @PreviousStatus = 'Rejected'
    BEGIN
        THROW 58012, 'Rejected payment cannot move to operational processing states.', 1;
    END;

    IF @PaymentStatus = 'Processing' AND @PreviousStatus NOT IN ('Approved', 'Processing')
    BEGIN
        THROW 58013, 'Payment must be approved before moving to processing.', 1;
    END;

    IF @PaymentStatus IN ('Paid', 'Failed') AND @PreviousStatus NOT IN ('Approved', 'Processing', 'Paid', 'Failed')
    BEGIN
        THROW 58014, 'Payment must be approved or processing before final status updates.', 1;
    END;

    UPDATE dbo.ClaimPayments
    SET PaymentStatus = @PaymentStatus,
        StatusNote = @StatusNote,
        LastStatusUpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimPaymentId = @ClaimPaymentId;

    INSERT INTO dbo.ClaimPaymentStatusHistory
    (
        ClaimPaymentStatusHistoryId,
        ClaimPaymentId,
        PreviousStatus,
        NewStatus,
        Note,
        ChangedByUserId,
        ChangedAtUtc
    )
    VALUES
    (
        NEWID(),
        @ClaimPaymentId,
        @PreviousStatus,
        @PaymentStatus,
        @StatusNote,
        @ChangedByUserId,
        SYSUTCDATETIME()
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
        'PaymentStatusUpdated',
        @PreviousStatus,
        @PaymentStatus,
        @ChangedByUserId,
        LEFT(ISNULL(@StatusNote, 'Payment status updated'), 500)
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Settlement_GetPaymentStatusHistory
    @ClaimPaymentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.ClaimPaymentStatusHistoryId,
        h.ClaimPaymentId,
        h.PreviousStatus,
        h.NewStatus,
        h.Note,
        h.ChangedByUserId,
        h.ChangedAtUtc
    FROM dbo.ClaimPaymentStatusHistory h
    WHERE h.ClaimPaymentId = @ClaimPaymentId
    ORDER BY h.ChangedAtUtc DESC;
END;
GO
