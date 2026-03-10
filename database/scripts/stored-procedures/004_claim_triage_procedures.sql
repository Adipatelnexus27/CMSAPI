CREATE OR ALTER PROCEDURE dbo.sp_Claims_Create
    @ClaimId UNIQUEIDENTIFIER,
    @ClaimNumber NVARCHAR(50),
    @PolicyNumber NVARCHAR(50),
    @ClaimType NVARCHAR(100),
    @ReporterName NVARCHAR(200),
    @IncidentDateUtc DATETIME2,
    @IncidentLocation NVARCHAR(300),
    @IncidentDescription NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.PolicyRegistry p
        WHERE p.PolicyNumber = @PolicyNumber
          AND p.IsActive = 1
          AND @IncidentDateUtc >= p.EffectiveDateUtc
          AND @IncidentDateUtc <= p.ExpiryDateUtc
    )
    BEGIN
        THROW 53001, 'Policy is invalid or inactive for the incident date.', 1;
    END;

    INSERT INTO dbo.Claims
    (
        ClaimId,
        ClaimNumber,
        PolicyNumber,
        ClaimType,
        ClaimStatus,
        Priority,
        WorkflowStep,
        ReporterName,
        IncidentDateUtc,
        IncidentLocation,
        IncidentDescription
    )
    VALUES
    (
        @ClaimId,
        @ClaimNumber,
        @PolicyNumber,
        @ClaimType,
        'New',
        2,
        'Registration',
        @ReporterName,
        @IncidentDateUtc,
        @IncidentLocation,
        @IncidentDescription
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
        'ClaimCreated',
        NULL,
        'New',
        NULL,
        'Claim created through intake registration'
    );

    SELECT
        ClaimId,
        ClaimNumber,
        PolicyNumber,
        ClaimType,
        ClaimStatus,
        Priority,
        WorkflowStep,
        InvestigatorUserId,
        AdjusterUserId,
        ReporterName,
        IncidentDateUtc,
        CreatedAtUtc
    FROM dbo.Claims
    WHERE ClaimId = @ClaimId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GetList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClaimId,
        ClaimNumber,
        PolicyNumber,
        ClaimType,
        ClaimStatus,
        Priority,
        WorkflowStep,
        InvestigatorUserId,
        AdjusterUserId,
        ReporterName,
        IncidentDateUtc,
        CreatedAtUtc
    FROM dbo.Claims
    ORDER BY CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GetById
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        ClaimId,
        ClaimNumber,
        PolicyNumber,
        ClaimType,
        ClaimStatus,
        Priority,
        WorkflowStep,
        InvestigatorUserId,
        AdjusterUserId,
        ReporterName,
        IncidentDateUtc,
        IncidentLocation,
        IncidentDescription,
        CreatedAtUtc
    FROM dbo.Claims
    WHERE ClaimId = @ClaimId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_AssignInvestigator
    @ClaimId UNIQUEIDENTIFIER,
    @InvestigatorUserId UNIQUEIDENTIFIER,
    @ChangedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Claims WHERE ClaimId = @ClaimId)
    BEGIN
        THROW 54001, 'Claim not found.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.UserRoles ur
        INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
        WHERE ur.UserId = @InvestigatorUserId
          AND r.Name = 'Investigator'
    )
    BEGIN
        THROW 54002, 'Assigned investigator must have Investigator role.', 1;
    END;

    DECLARE @PreviousValue NVARCHAR(300);
    SELECT @PreviousValue = CONVERT(NVARCHAR(300), InvestigatorUserId) FROM dbo.Claims WHERE ClaimId = @ClaimId;

    UPDATE dbo.Claims
    SET InvestigatorUserId = @InvestigatorUserId,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimId = @ClaimId;

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
        'AssignInvestigator',
        @PreviousValue,
        CONVERT(NVARCHAR(300), @InvestigatorUserId),
        @ChangedByUserId,
        'Investigator assignment updated'
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_AssignAdjuster
    @ClaimId UNIQUEIDENTIFIER,
    @AdjusterUserId UNIQUEIDENTIFIER,
    @ChangedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Claims WHERE ClaimId = @ClaimId)
    BEGIN
        THROW 54001, 'Claim not found.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.UserRoles ur
        INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
        WHERE ur.UserId = @AdjusterUserId
          AND r.Name = 'Adjuster'
    )
    BEGIN
        THROW 54003, 'Assigned adjuster must have Adjuster role.', 1;
    END;

    DECLARE @PreviousValue NVARCHAR(300);
    SELECT @PreviousValue = CONVERT(NVARCHAR(300), AdjusterUserId) FROM dbo.Claims WHERE ClaimId = @ClaimId;

    UPDATE dbo.Claims
    SET AdjusterUserId = @AdjusterUserId,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimId = @ClaimId;

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
        'AssignAdjuster',
        @PreviousValue,
        CONVERT(NVARCHAR(300), @AdjusterUserId),
        @ChangedByUserId,
        'Adjuster assignment updated'
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_SetPriority
    @ClaimId UNIQUEIDENTIFIER,
    @Priority INT,
    @ChangedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Priority < 1 OR @Priority > 5
    BEGIN
        THROW 54004, 'Priority must be between 1 and 5.', 1;
    END;

    DECLARE @PreviousPriority NVARCHAR(300);
    SELECT @PreviousPriority = CONVERT(NVARCHAR(300), Priority) FROM dbo.Claims WHERE ClaimId = @ClaimId;

    UPDATE dbo.Claims
    SET Priority = @Priority,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimId = @ClaimId;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 54001, 'Claim not found.', 1;
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
        'SetPriority',
        @PreviousPriority,
        CONVERT(NVARCHAR(300), @Priority),
        @ChangedByUserId,
        'Claim priority updated'
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_UpdateStatus
    @ClaimId UNIQUEIDENTIFIER,
    @ClaimStatus NVARCHAR(100),
    @ChangedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.ConfigurationItems ci
        WHERE ci.ConfigType = 'ClaimStatus'
          AND ci.Name = @ClaimStatus
          AND ci.IsActive = 1
    )
    BEGIN
        THROW 54005, 'Invalid or inactive claim status.', 1;
    END;

    DECLARE @PreviousStatus NVARCHAR(300);
    SELECT @PreviousStatus = ClaimStatus FROM dbo.Claims WHERE ClaimId = @ClaimId;

    UPDATE dbo.Claims
    SET ClaimStatus = @ClaimStatus,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimId = @ClaimId;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 54001, 'Claim not found.', 1;
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
        'UpdateStatus',
        @PreviousStatus,
        @ClaimStatus,
        @ChangedByUserId,
        'Claim status updated'
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_UpdateWorkflowStep
    @ClaimId UNIQUEIDENTIFIER,
    @WorkflowStep NVARCHAR(100),
    @ChangedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.WorkflowSettings WHERE WorkflowKey = 'Claim' AND IsActive = 1)
       AND NOT EXISTS (SELECT 1 FROM dbo.WorkflowSettings WHERE WorkflowKey = 'Claim' AND StepName = @WorkflowStep AND IsActive = 1)
    BEGIN
        THROW 54006, 'Workflow step is not configured in active claim workflow settings.', 1;
    END;

    DECLARE @PreviousStep NVARCHAR(300);
    SELECT @PreviousStep = WorkflowStep FROM dbo.Claims WHERE ClaimId = @ClaimId;

    UPDATE dbo.Claims
    SET WorkflowStep = @WorkflowStep,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimId = @ClaimId;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 54001, 'Claim not found.', 1;
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
        'UpdateWorkflowStep',
        @PreviousStep,
        @WorkflowStep,
        @ChangedByUserId,
        'Workflow step updated'
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GetAssignedClaims
    @AssigneeUserId UNIQUEIDENTIFIER,
    @Role NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF UPPER(@Role) = 'INVESTIGATOR'
    BEGIN
        SELECT
            ClaimId,
            ClaimNumber,
            PolicyNumber,
            ClaimType,
            ClaimStatus,
            Priority,
            WorkflowStep,
            InvestigatorUserId,
            AdjusterUserId,
            ReporterName,
            IncidentDateUtc,
            CreatedAtUtc
        FROM dbo.Claims
        WHERE InvestigatorUserId = @AssigneeUserId
        ORDER BY CreatedAtUtc DESC;
        RETURN;
    END;

    IF UPPER(@Role) = 'ADJUSTER'
    BEGIN
        SELECT
            ClaimId,
            ClaimNumber,
            PolicyNumber,
            ClaimType,
            ClaimStatus,
            Priority,
            WorkflowStep,
            InvestigatorUserId,
            AdjusterUserId,
            ReporterName,
            IncidentDateUtc,
            CreatedAtUtc
        FROM dbo.Claims
        WHERE AdjusterUserId = @AssigneeUserId
        ORDER BY CreatedAtUtc DESC;
        RETURN;
    END;

    THROW 54007, 'Role must be Investigator or Adjuster.', 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GetWorkflowHistory
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClaimWorkflowHistoryId,
        ClaimId,
        ActionType,
        PreviousValue,
        NewValue,
        ChangedByUserId,
        ChangedAtUtc
    FROM dbo.ClaimWorkflowHistory
    WHERE ClaimId = @ClaimId
    ORDER BY ChangedAtUtc DESC;
END;
GO
