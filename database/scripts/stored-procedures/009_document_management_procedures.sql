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
        UploadedAtUtc,
        DocumentGroupId,
        VersionNumber,
        IsLatest,
        UploadedByUserId
    FROM dbo.ClaimDocuments
    WHERE ClaimId = @ClaimId
    ORDER BY UploadedAtUtc DESC;
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
        UploadedAtUtc,
        DocumentGroupId,
        VersionNumber,
        IsLatest,
        UploadedByUserId
    FROM dbo.ClaimDocuments
    WHERE ClaimId = @ClaimId
      AND DocumentCategory IN ('Evidence', 'AccidentPhoto', 'PoliceReport', 'MedicalReport')
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
    @DocumentCategory NVARCHAR(50) = 'General',
    @DocumentGroupId UNIQUEIDENTIFIER = NULL,
    @UploadedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Claims WHERE ClaimId = @ClaimId)
    BEGIN
        THROW 59001, 'Claim not found.', 1;
    END;

    DECLARE @NormalizedCategory NVARCHAR(50) = LTRIM(RTRIM(ISNULL(@DocumentCategory, 'General')));
    IF @NormalizedCategory = ''
    BEGIN
        SET @NormalizedCategory = 'General';
    END;

    IF @NormalizedCategory NOT IN ('General', 'Evidence', 'AccidentPhoto', 'PoliceReport', 'MedicalReport', 'Invoice', 'Settlement')
    BEGIN
        THROW 59002, 'Invalid document category.', 1;
    END;

    DECLARE @ResolvedGroupId UNIQUEIDENTIFIER = ISNULL(@DocumentGroupId, NEWID());
    DECLARE @NextVersion INT = 1;

    BEGIN TRANSACTION;

    SELECT @NextVersion = ISNULL(MAX(VersionNumber), 0) + 1
    FROM dbo.ClaimDocuments
    WHERE DocumentGroupId = @ResolvedGroupId;

    UPDATE dbo.ClaimDocuments
    SET IsLatest = 0
    WHERE DocumentGroupId = @ResolvedGroupId
      AND IsLatest = 1;

    INSERT INTO dbo.ClaimDocuments
    (
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        StoredFilePath,
        ContentType,
        FileSizeBytes,
        DocumentCategory,
        DocumentGroupId,
        VersionNumber,
        IsLatest,
        UploadedByUserId
    )
    VALUES
    (
        @ClaimDocumentId,
        @ClaimId,
        @OriginalFileName,
        @StoredFilePath,
        @ContentType,
        @FileSizeBytes,
        @NormalizedCategory,
        @ResolvedGroupId,
        @NextVersion,
        1,
        @UploadedByUserId
    );

    COMMIT TRANSACTION;

    SELECT
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        StoredFilePath,
        DocumentCategory,
        ContentType,
        FileSizeBytes,
        UploadedAtUtc,
        DocumentGroupId,
        VersionNumber,
        IsLatest,
        UploadedByUserId
    FROM dbo.ClaimDocuments
    WHERE ClaimDocumentId = @ClaimDocumentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Documents_GetByClaimId
    @ClaimId UNIQUEIDENTIFIER,
    @LatestOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        StoredFilePath,
        DocumentCategory,
        ContentType,
        FileSizeBytes,
        UploadedAtUtc,
        DocumentGroupId,
        VersionNumber,
        IsLatest,
        UploadedByUserId
    FROM dbo.ClaimDocuments
    WHERE ClaimId = @ClaimId
      AND (@LatestOnly = 0 OR IsLatest = 1)
    ORDER BY UploadedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Documents_GetById
    @ClaimDocumentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        StoredFilePath,
        DocumentCategory,
        ContentType,
        FileSizeBytes,
        UploadedAtUtc,
        DocumentGroupId,
        VersionNumber,
        IsLatest,
        UploadedByUserId
    FROM dbo.ClaimDocuments
    WHERE ClaimDocumentId = @ClaimDocumentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Documents_GetVersionsByGroup
    @DocumentGroupId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        StoredFilePath,
        DocumentCategory,
        ContentType,
        FileSizeBytes,
        UploadedAtUtc,
        DocumentGroupId,
        VersionNumber,
        IsLatest,
        UploadedByUserId
    FROM dbo.ClaimDocuments
    WHERE DocumentGroupId = @DocumentGroupId
    ORDER BY VersionNumber DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Documents_AddVersion
    @ClaimDocumentId UNIQUEIDENTIFIER,
    @ClaimId UNIQUEIDENTIFIER,
    @OriginalFileName NVARCHAR(300),
    @StoredFilePath NVARCHAR(1000),
    @ContentType NVARCHAR(100),
    @FileSizeBytes BIGINT,
    @DocumentCategory NVARCHAR(50) = 'General',
    @DocumentGroupId UNIQUEIDENTIFIER = NULL,
    @UploadedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    EXEC dbo.sp_Claims_AddDocument
        @ClaimDocumentId = @ClaimDocumentId,
        @ClaimId = @ClaimId,
        @OriginalFileName = @OriginalFileName,
        @StoredFilePath = @StoredFilePath,
        @ContentType = @ContentType,
        @FileSizeBytes = @FileSizeBytes,
        @DocumentCategory = @DocumentCategory,
        @DocumentGroupId = @DocumentGroupId,
        @UploadedByUserId = @UploadedByUserId;
END;
GO
