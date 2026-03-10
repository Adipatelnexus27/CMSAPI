CREATE OR ALTER PROCEDURE dbo.sp_Claims_ValidatePolicy
    @PolicyNumber NVARCHAR(50),
    @IncidentDateUtc DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.PolicyRegistry p
        WHERE p.PolicyNumber = @PolicyNumber
          AND p.IsActive = 1
          AND @IncidentDateUtc >= p.EffectiveDateUtc
          AND @IncidentDateUtc <= p.ExpiryDateUtc
    )
    BEGIN
        SELECT CAST(1 AS INT);
        RETURN;
    END;

    SELECT CAST(0 AS INT);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GenerateClaimNumber
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NextValue BIGINT = NEXT VALUE FOR dbo.Seq_ClaimNumber;
    DECLARE @YearPart NVARCHAR(4) = CONVERT(NVARCHAR(4), DATEPART(YEAR, SYSUTCDATETIME()));
    DECLARE @SeqPart NVARCHAR(10) = RIGHT(REPLICATE('0', 6) + CONVERT(NVARCHAR(20), @NextValue), 6);

    SELECT CONCAT('CLM-', @YearPart, '-', @SeqPart);
END;
GO

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
        @ReporterName,
        @IncidentDateUtc,
        @IncidentLocation,
        @IncidentDescription
    );

    SELECT
        ClaimId,
        ClaimNumber,
        PolicyNumber,
        ClaimType,
        ClaimStatus,
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
        ReporterName,
        IncidentDateUtc,
        IncidentLocation,
        IncidentDescription,
        CreatedAtUtc
    FROM dbo.Claims
    WHERE ClaimId = @ClaimId;
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
    @FileSizeBytes BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ClaimDocuments
    (
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        StoredFilePath,
        ContentType,
        FileSizeBytes
    )
    VALUES
    (
        @ClaimDocumentId,
        @ClaimId,
        @OriginalFileName,
        @StoredFilePath,
        @ContentType,
        @FileSizeBytes
    );

    SELECT
        ClaimDocumentId,
        ClaimId,
        OriginalFileName,
        ContentType,
        FileSizeBytes,
        UploadedAtUtc
    FROM dbo.ClaimDocuments
    WHERE ClaimDocumentId = @ClaimDocumentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_LinkRelated
    @ClaimId UNIQUEIDENTIFIER,
    @RelatedClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF @ClaimId = @RelatedClaimId
    BEGIN
        THROW 53002, 'Claim cannot be linked to itself.', 1;
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Claims WHERE ClaimId = @ClaimId)
    BEGIN
        THROW 53003, 'Primary claim not found.', 1;
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Claims WHERE ClaimId = @RelatedClaimId)
    BEGIN
        THROW 53004, 'Related claim not found.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.ClaimRelations
        WHERE (ClaimId = @ClaimId AND RelatedClaimId = @RelatedClaimId)
           OR (ClaimId = @RelatedClaimId AND RelatedClaimId = @ClaimId)
    )
    BEGIN
        INSERT INTO dbo.ClaimRelations(ClaimRelationId, ClaimId, RelatedClaimId)
        VALUES (NEWID(), @ClaimId, @RelatedClaimId);
    END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Claims_GetRelatedClaims
    @ClaimId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.ClaimId, c.ClaimNumber, c.ClaimStatus
    FROM dbo.ClaimRelations cr
    INNER JOIN dbo.Claims c ON c.ClaimId = cr.RelatedClaimId
    WHERE cr.ClaimId = @ClaimId

    UNION

    SELECT c.ClaimId, c.ClaimNumber, c.ClaimStatus
    FROM dbo.ClaimRelations cr
    INNER JOIN dbo.Claims c ON c.ClaimId = cr.ClaimId
    WHERE cr.RelatedClaimId = @ClaimId;
END;
GO
