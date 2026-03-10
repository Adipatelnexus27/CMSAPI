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
        InvestigationProgress,
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
        0,
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
        CreatedAtUtc,
        InvestigationProgress
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
        CreatedAtUtc,
        InvestigationProgress
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
        CreatedAtUtc,
        InvestigationProgress
    FROM dbo.Claims
    WHERE ClaimId = @ClaimId;
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
            CreatedAtUtc,
            InvestigationProgress
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
            CreatedAtUtc,
            InvestigationProgress
        FROM dbo.Claims
        WHERE AdjusterUserId = @AssigneeUserId
        ORDER BY CreatedAtUtc DESC;
        RETURN;
    END;

    THROW 54007, 'Role must be Investigator or Adjuster.', 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GetDocuments
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        DocumentCategory,
        ContentType,
        FileSizeBytes,
        UploadedAtUtc
    FROM dbo.ClaimDocuments
    WHERE ClaimId = @ClaimId
    ORDER BY UploadedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_AddDocument
    @ClaimDocumentId UNIQUEIDENTIFIER,
    @ClaimId UNIQUEIDENTIFIER,
    @OriginalFileName NVARCHAR(300),
    @StoredFilePath NVARCHAR(1000),
    @ContentType NVARCHAR(100),
    @FileSizeBytes BIGINT,
    @DocumentCategory NVARCHAR(50) = 'General'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NormalizedCategory NVARCHAR(50) = LTRIM(RTRIM(ISNULL(@DocumentCategory, 'General')));
    IF @NormalizedCategory = ''
    BEGIN
        SET @NormalizedCategory = 'General';
    END;

    IF @NormalizedCategory NOT IN ('General', 'Evidence', 'AccidentPhoto', 'PoliceReport', 'MedicalReport')
    BEGIN
        THROW 55001, 'Invalid document category.', 1;
    END;

    INSERT INTO dbo.ClaimDocuments
    (
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        StoredFilePath,
        ContentType,
        FileSizeBytes,
        DocumentCategory
    )
    VALUES
    (
        @ClaimDocumentId,
        @ClaimId,
        @OriginalFileName,
        @StoredFilePath,
        @ContentType,
        @FileSizeBytes,
        @NormalizedCategory
    );

    SELECT
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        DocumentCategory,
        ContentType,
        FileSizeBytes,
        UploadedAtUtc
    FROM dbo.ClaimDocuments
    WHERE ClaimDocumentId = @ClaimDocumentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GetInvestigationDocuments
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        DocumentCategory,
        ContentType,
        FileSizeBytes,
        UploadedAtUtc
    FROM dbo.ClaimDocuments
    WHERE ClaimId = @ClaimId
      AND DocumentCategory IN ('Evidence', 'AccidentPhoto', 'PoliceReport', 'MedicalReport')
    ORDER BY UploadedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_AddInvestigatorNote
    @ClaimInvestigationNoteId UNIQUEIDENTIFIER,
    @ClaimId UNIQUEIDENTIFIER,
    @NoteText NVARCHAR(MAX),
    @ProgressPercentSnapshot INT = NULL,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Claims WHERE ClaimId = @ClaimId)
    BEGIN
        THROW 55002, 'Claim not found.', 1;
    END;

    DECLARE @NormalizedNote NVARCHAR(MAX) = LTRIM(RTRIM(ISNULL(@NoteText, '')));
    IF @NormalizedNote = ''
    BEGIN
        THROW 55003, 'Investigator note is required.', 1;
    END;

    IF @ProgressPercentSnapshot IS NOT NULL AND (@ProgressPercentSnapshot < 0 OR @ProgressPercentSnapshot > 100)
    BEGIN
        THROW 55004, 'Investigation progress must be between 0 and 100.', 1;
    END;

    INSERT INTO dbo.ClaimInvestigationNotes
    (
        ClaimInvestigationNoteId,
        ClaimId,
        NoteText,
        ProgressPercentSnapshot,
        CreatedByUserId
    )
    VALUES
    (
        @ClaimInvestigationNoteId,
        @ClaimId,
        @NormalizedNote,
        @ProgressPercentSnapshot,
        @CreatedByUserId
    );

    IF @ProgressPercentSnapshot IS NOT NULL
    BEGIN
        DECLARE @PreviousProgress NVARCHAR(300);
        SELECT @PreviousProgress = CONVERT(NVARCHAR(300), InvestigationProgress)
        FROM dbo.Claims
        WHERE ClaimId = @ClaimId;

        UPDATE dbo.Claims
        SET InvestigationProgress = @ProgressPercentSnapshot,
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
            'UpdateInvestigationProgress',
            @PreviousProgress,
            CONVERT(NVARCHAR(300), @ProgressPercentSnapshot),
            @CreatedByUserId,
            'Investigation progress updated from investigator note'
        );
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
        'AddInvestigatorNote',
        NULL,
        LEFT(@NormalizedNote, 300),
        @CreatedByUserId,
        'Investigator note added'
    );

    SELECT
        ClaimInvestigationNoteId,
        ClaimId,
        NoteText,
        ProgressPercentSnapshot,
        CreatedByUserId,
        CreatedAtUtc
    FROM dbo.ClaimInvestigationNotes
    WHERE ClaimInvestigationNoteId = @ClaimInvestigationNoteId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GetInvestigationNotes
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClaimInvestigationNoteId,
        ClaimId,
        NoteText,
        ProgressPercentSnapshot,
        CreatedByUserId,
        CreatedAtUtc
    FROM dbo.ClaimInvestigationNotes
    WHERE ClaimId = @ClaimId
    ORDER BY CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_UpdateInvestigationProgress
    @ClaimId UNIQUEIDENTIFIER,
    @ProgressPercent INT,
    @ChangedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @ProgressPercent < 0 OR @ProgressPercent > 100
    BEGIN
        THROW 55004, 'Investigation progress must be between 0 and 100.', 1;
    END;

    DECLARE @PreviousProgress NVARCHAR(300);
    SELECT @PreviousProgress = CONVERT(NVARCHAR(300), InvestigationProgress)
    FROM dbo.Claims
    WHERE ClaimId = @ClaimId;

    UPDATE dbo.Claims
    SET InvestigationProgress = @ProgressPercent,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE ClaimId = @ClaimId;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 55002, 'Claim not found.', 1;
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
        'UpdateInvestigationProgress',
        @PreviousProgress,
        CONVERT(NVARCHAR(300), @ProgressPercent),
        @ChangedByUserId,
        'Investigation progress updated'
    );
END;
GO
