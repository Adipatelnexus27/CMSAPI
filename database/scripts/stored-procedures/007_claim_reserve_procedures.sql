CREATE OR ALTER PROCEDURE dbo.sp_Reserve_CreateInitial
    @ClaimId UNIQUEIDENTIFIER,
    @ReserveAmount DECIMAL(18,2),
    @CurrencyCode NVARCHAR(3),
    @Reason NVARCHAR(500) = NULL,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @ReserveAmount <= 0
    BEGIN
        THROW 57001, 'Reserve amount must be greater than 0.', 1;
    END;

    IF LEN(ISNULL(@CurrencyCode, '')) <> 3
    BEGIN
        THROW 57002, 'Currency code must be a 3-letter code.', 1;
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Claims WHERE ClaimId = @ClaimId)
    BEGIN
        THROW 57003, 'Claim not found.', 1;
    END;

    IF EXISTS (SELECT 1 FROM dbo.ClaimReserves WHERE ClaimId = @ClaimId)
    BEGIN
        THROW 57004, 'Initial reserve already exists for this claim.', 1;
    END;

    DECLARE @ClaimReserveId UNIQUEIDENTIFIER = NEWID();
    DECLARE @NormalizedCurrency NVARCHAR(3) = UPPER(@CurrencyCode);

    INSERT INTO dbo.ClaimReserves
    (
        ClaimReserveId,
        ClaimId,
        CurrentReserveAmount,
        CurrencyCode,
        LastApprovedAtUtc,
        LastApprovedByUserId,
        CreatedByUserId,
        UpdatedAtUtc
    )
    VALUES
    (
        @ClaimReserveId,
        @ClaimId,
        @ReserveAmount,
        @NormalizedCurrency,
        SYSUTCDATETIME(),
        @CreatedByUserId,
        @CreatedByUserId,
        SYSUTCDATETIME()
    );

    INSERT INTO dbo.ClaimReserveHistory
    (
        ClaimReserveHistoryId,
        ClaimReserveId,
        ClaimId,
        ActionType,
        PreviousReserveAmount,
        RequestedReserveAmount,
        ApprovedReserveAmount,
        CurrencyCode,
        Status,
        Reason,
        RequestedByUserId,
        RequestedAtUtc,
        ApprovedByUserId,
        ApprovedAtUtc,
        ApprovalNote
    )
    VALUES
    (
        NEWID(),
        @ClaimReserveId,
        @ClaimId,
        'InitialReserve',
        NULL,
        @ReserveAmount,
        @ReserveAmount,
        @NormalizedCurrency,
        'Approved',
        @Reason,
        @CreatedByUserId,
        SYSUTCDATETIME(),
        @CreatedByUserId,
        SYSUTCDATETIME(),
        'Initial reserve created'
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
        'InitialReserveCreated',
        NULL,
        CONVERT(NVARCHAR(300), @ReserveAmount),
        @CreatedByUserId,
        LEFT(ISNULL(@Reason, 'Initial reserve created'), 500)
    );

    SELECT
        cr.ClaimReserveId,
        cr.ClaimId,
        c.ClaimNumber,
        cr.CurrentReserveAmount,
        cr.CurrencyCode,
        cr.LastApprovedAtUtc,
        cr.LastApprovedByUserId,
        cr.CreatedAtUtc,
        cr.UpdatedAtUtc
    FROM dbo.ClaimReserves cr
    INNER JOIN dbo.Claims c ON c.ClaimId = cr.ClaimId
    WHERE cr.ClaimReserveId = @ClaimReserveId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserve_RequestAdjustment
    @ClaimId UNIQUEIDENTIFIER,
    @ReserveAmount DECIMAL(18,2),
    @Reason NVARCHAR(500) = NULL,
    @RequestedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @ReserveAmount <= 0
    BEGIN
        THROW 57001, 'Reserve amount must be greater than 0.', 1;
    END;

    DECLARE @ClaimReserveId UNIQUEIDENTIFIER;
    DECLARE @PreviousReserveAmount DECIMAL(18,2);
    DECLARE @CurrencyCode NVARCHAR(3);

    SELECT
        @ClaimReserveId = cr.ClaimReserveId,
        @PreviousReserveAmount = cr.CurrentReserveAmount,
        @CurrencyCode = cr.CurrencyCode
    FROM dbo.ClaimReserves cr
    WHERE cr.ClaimId = @ClaimId;

    IF @ClaimReserveId IS NULL
    BEGIN
        THROW 57005, 'Initial reserve must be created before adjustment.', 1;
    END;

    IF @PreviousReserveAmount = @ReserveAmount
    BEGIN
        THROW 57006, 'Adjusted reserve amount must be different from current reserve amount.', 1;
    END;

    DECLARE @ClaimReserveHistoryId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.ClaimReserveHistory
    (
        ClaimReserveHistoryId,
        ClaimReserveId,
        ClaimId,
        ActionType,
        PreviousReserveAmount,
        RequestedReserveAmount,
        ApprovedReserveAmount,
        CurrencyCode,
        Status,
        Reason,
        RequestedByUserId,
        RequestedAtUtc,
        ApprovedByUserId,
        ApprovedAtUtc,
        ApprovalNote
    )
    VALUES
    (
        @ClaimReserveHistoryId,
        @ClaimReserveId,
        @ClaimId,
        'AdjustmentRequested',
        @PreviousReserveAmount,
        @ReserveAmount,
        NULL,
        @CurrencyCode,
        'PendingApproval',
        @Reason,
        @RequestedByUserId,
        SYSUTCDATETIME(),
        NULL,
        NULL,
        NULL
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
        'ReserveAdjustmentRequested',
        CONVERT(NVARCHAR(300), @PreviousReserveAmount),
        CONVERT(NVARCHAR(300), @ReserveAmount),
        @RequestedByUserId,
        LEFT(ISNULL(@Reason, 'Reserve adjustment submitted for approval'), 500)
    );

    SELECT
        h.ClaimReserveHistoryId,
        h.ClaimReserveId,
        h.ClaimId,
        c.ClaimNumber,
        h.ActionType,
        h.PreviousReserveAmount,
        h.RequestedReserveAmount,
        h.ApprovedReserveAmount,
        h.CurrencyCode,
        h.Status,
        h.Reason,
        h.RequestedByUserId,
        h.RequestedAtUtc,
        h.ApprovedByUserId,
        h.ApprovedAtUtc,
        h.ApprovalNote
    FROM dbo.ClaimReserveHistory h
    INNER JOIN dbo.Claims c ON c.ClaimId = h.ClaimId
    WHERE h.ClaimReserveHistoryId = @ClaimReserveHistoryId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserve_GetByClaimId
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        cr.ClaimReserveId,
        cr.ClaimId,
        c.ClaimNumber,
        cr.CurrentReserveAmount,
        cr.CurrencyCode,
        cr.LastApprovedAtUtc,
        cr.LastApprovedByUserId,
        cr.CreatedAtUtc,
        cr.UpdatedAtUtc
    FROM dbo.ClaimReserves cr
    INNER JOIN dbo.Claims c ON c.ClaimId = cr.ClaimId
    WHERE cr.ClaimId = @ClaimId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserve_GetHistory
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.ClaimReserveHistoryId,
        h.ClaimReserveId,
        h.ClaimId,
        c.ClaimNumber,
        h.ActionType,
        h.PreviousReserveAmount,
        h.RequestedReserveAmount,
        h.ApprovedReserveAmount,
        h.CurrencyCode,
        h.Status,
        h.Reason,
        h.RequestedByUserId,
        h.RequestedAtUtc,
        h.ApprovedByUserId,
        h.ApprovedAtUtc,
        h.ApprovalNote
    FROM dbo.ClaimReserveHistory h
    INNER JOIN dbo.Claims c ON c.ClaimId = h.ClaimId
    WHERE h.ClaimId = @ClaimId
    ORDER BY h.RequestedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserve_GetApprovalQueue
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.ClaimReserveHistoryId,
        h.ClaimReserveId,
        h.ClaimId,
        c.ClaimNumber,
        h.ActionType,
        h.PreviousReserveAmount,
        h.RequestedReserveAmount,
        h.ApprovedReserveAmount,
        h.CurrencyCode,
        h.Status,
        h.Reason,
        h.RequestedByUserId,
        h.RequestedAtUtc,
        h.ApprovedByUserId,
        h.ApprovedAtUtc,
        h.ApprovalNote
    FROM dbo.ClaimReserveHistory h
    INNER JOIN dbo.Claims c ON c.ClaimId = h.ClaimId
    WHERE (@Status IS NULL OR h.Status = @Status)
    ORDER BY h.RequestedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserve_ApproveAdjustment
    @ClaimReserveHistoryId UNIQUEIDENTIFIER,
    @ApprovalNote NVARCHAR(1000) = NULL,
    @ApprovedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ClaimReserveId UNIQUEIDENTIFIER;
    DECLARE @ClaimId UNIQUEIDENTIFIER;
    DECLARE @PreviousReserveAmount DECIMAL(18,2);
    DECLARE @RequestedReserveAmount DECIMAL(18,2);

    SELECT
        @ClaimReserveId = h.ClaimReserveId,
        @ClaimId = h.ClaimId,
        @PreviousReserveAmount = h.PreviousReserveAmount,
        @RequestedReserveAmount = h.RequestedReserveAmount
    FROM dbo.ClaimReserveHistory h
    WHERE h.ClaimReserveHistoryId = @ClaimReserveHistoryId
      AND h.Status = 'PendingApproval';

    IF @ClaimReserveId IS NULL
    BEGIN
        THROW 57007, 'Pending reserve adjustment not found.', 1;
    END;

    UPDATE dbo.ClaimReserves
    SET CurrentReserveAmount = @RequestedReserveAmount,
        LastApprovedAtUtc = SYSUTCDATETIME(),
        LastApprovedByUserId = @ApprovedByUserId,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimReserveId = @ClaimReserveId;

    UPDATE dbo.ClaimReserveHistory
    SET ActionType = 'AdjustmentApproved',
        Status = 'Approved',
        ApprovedReserveAmount = @RequestedReserveAmount,
        ApprovedByUserId = @ApprovedByUserId,
        ApprovedAtUtc = SYSUTCDATETIME(),
        ApprovalNote = @ApprovalNote
    WHERE ClaimReserveHistoryId = @ClaimReserveHistoryId;

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
        'ReserveAdjustmentApproved',
        CONVERT(NVARCHAR(300), @PreviousReserveAmount),
        CONVERT(NVARCHAR(300), @RequestedReserveAmount),
        @ApprovedByUserId,
        LEFT(ISNULL(@ApprovalNote, 'Reserve adjustment approved'), 500)
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserve_RejectAdjustment
    @ClaimReserveHistoryId UNIQUEIDENTIFIER,
    @ApprovalNote NVARCHAR(1000) = NULL,
    @ApprovedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ClaimId UNIQUEIDENTIFIER;
    DECLARE @PreviousReserveAmount DECIMAL(18,2);
    DECLARE @RequestedReserveAmount DECIMAL(18,2);

    SELECT
        @ClaimId = h.ClaimId,
        @PreviousReserveAmount = h.PreviousReserveAmount,
        @RequestedReserveAmount = h.RequestedReserveAmount
    FROM dbo.ClaimReserveHistory h
    WHERE h.ClaimReserveHistoryId = @ClaimReserveHistoryId
      AND h.Status = 'PendingApproval';

    IF @ClaimId IS NULL
    BEGIN
        THROW 57007, 'Pending reserve adjustment not found.', 1;
    END;

    UPDATE dbo.ClaimReserveHistory
    SET ActionType = 'AdjustmentRejected',
        Status = 'Rejected',
        ApprovedReserveAmount = NULL,
        ApprovedByUserId = @ApprovedByUserId,
        ApprovedAtUtc = SYSUTCDATETIME(),
        ApprovalNote = @ApprovalNote
    WHERE ClaimReserveHistoryId = @ClaimReserveHistoryId;

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
        'ReserveAdjustmentRejected',
        CONVERT(NVARCHAR(300), @PreviousReserveAmount),
        CONVERT(NVARCHAR(300), @RequestedReserveAmount),
        @ApprovedByUserId,
        LEFT(ISNULL(@ApprovalNote, 'Reserve adjustment rejected'), 500)
    );
END;
GO
