CREATE OR ALTER PROCEDURE dbo.sp_AuditLogs_Insert
    @AuditLogId UNIQUEIDENTIFIER,
    @EventType NVARCHAR(50),
    @ActionName NVARCHAR(200),
    @EntityName NVARCHAR(100) = NULL,
    @EntityId UNIQUEIDENTIFIER = NULL,
    @ClaimId UNIQUEIDENTIFIER = NULL,
    @Description NVARCHAR(2000) = NULL,
    @RequestMethod NVARCHAR(10) = NULL,
    @RequestPath NVARCHAR(500) = NULL,
    @RequestQuery NVARCHAR(1000) = NULL,
    @HttpStatusCode INT = NULL,
    @IsSuccess BIT = 1,
    @DurationMs INT = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @UserEmail NVARCHAR(320) = NULL,
    @UserRoleCsv NVARCHAR(500) = NULL,
    @IpAddress NVARCHAR(100) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @CorrelationId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@EventType)), '') IS NULL
    BEGIN
        THROW 59001, 'EventType is required.', 1;
    END;

    IF NULLIF(LTRIM(RTRIM(@ActionName)), '') IS NULL
    BEGIN
        THROW 59002, 'ActionName is required.', 1;
    END;

    INSERT INTO dbo.AuditLogs
    (
        AuditLogId,
        EventType,
        ActionName,
        EntityName,
        EntityId,
        ClaimId,
        Description,
        RequestMethod,
        RequestPath,
        RequestQuery,
        HttpStatusCode,
        IsSuccess,
        DurationMs,
        UserId,
        UserEmail,
        UserRoleCsv,
        IpAddress,
        UserAgent,
        CorrelationId
    )
    VALUES
    (
        @AuditLogId,
        @EventType,
        @ActionName,
        @EntityName,
        @EntityId,
        @ClaimId,
        @Description,
        @RequestMethod,
        @RequestPath,
        @RequestQuery,
        @HttpStatusCode,
        @IsSuccess,
        @DurationMs,
        @UserId,
        @UserEmail,
        @UserRoleCsv,
        @IpAddress,
        @UserAgent,
        @CorrelationId
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_AuditLogs_GetList
    @FromDateUtc DATETIME2 = NULL,
    @ToDateUtc DATETIME2 = NULL,
    @EventType NVARCHAR(50) = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @ClaimId UNIQUEIDENTIFIER = NULL,
    @IsSuccess BIT = NULL,
    @ActionContains NVARCHAR(200) = NULL,
    @Take INT = 500
AS
BEGIN
    SET NOCOUNT ON;

    IF @Take IS NULL OR @Take < 1
    BEGIN
        SET @Take = 500;
    END;

    IF @Take > 2000
    BEGIN
        SET @Take = 2000;
    END;

    SELECT TOP (@Take)
        al.AuditLogId,
        al.EventType,
        al.ActionName,
        al.EntityName,
        al.EntityId,
        al.ClaimId,
        al.Description,
        al.RequestMethod,
        al.RequestPath,
        al.RequestQuery,
        al.HttpStatusCode,
        al.IsSuccess,
        al.DurationMs,
        al.UserId,
        al.UserEmail,
        al.UserRoleCsv,
        al.IpAddress,
        al.UserAgent,
        al.CorrelationId,
        al.CreatedAtUtc
    FROM dbo.AuditLogs al
    WHERE (@FromDateUtc IS NULL OR al.CreatedAtUtc >= @FromDateUtc)
      AND (@ToDateUtc IS NULL OR al.CreatedAtUtc <= @ToDateUtc)
      AND (@EventType IS NULL OR al.EventType = @EventType)
      AND (@UserId IS NULL OR al.UserId = @UserId)
      AND (@ClaimId IS NULL OR al.ClaimId = @ClaimId)
      AND (@IsSuccess IS NULL OR al.IsSuccess = @IsSuccess)
      AND (@ActionContains IS NULL OR al.ActionName LIKE '%' + @ActionContains + '%' OR ISNULL(al.Description, '') LIKE '%' + @ActionContains + '%')
    ORDER BY al.CreatedAtUtc DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_AuditLogs_GetReport
    @FromDateUtc DATETIME2 = NULL,
    @ToDateUtc DATETIME2 = NULL,
    @EventType NVARCHAR(50) = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @ClaimId UNIQUEIDENTIFIER = NULL,
    @IsSuccess BIT = NULL,
    @ActionContains NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Filtered AS
    (
        SELECT
            al.EventType,
            al.ActionName,
            al.ClaimId,
            al.UserId,
            al.IsSuccess
        FROM dbo.AuditLogs al
        WHERE (@FromDateUtc IS NULL OR al.CreatedAtUtc >= @FromDateUtc)
          AND (@ToDateUtc IS NULL OR al.CreatedAtUtc <= @ToDateUtc)
          AND (@EventType IS NULL OR al.EventType = @EventType)
          AND (@UserId IS NULL OR al.UserId = @UserId)
          AND (@ClaimId IS NULL OR al.ClaimId = @ClaimId)
          AND (@IsSuccess IS NULL OR al.IsSuccess = @IsSuccess)
          AND (@ActionContains IS NULL OR al.ActionName LIKE '%' + @ActionContains + '%')
    )
    SELECT
        COUNT(1) AS TotalEvents,
        SUM(CASE WHEN f.IsSuccess = 1 THEN 1 ELSE 0 END) AS SuccessfulEvents,
        SUM(CASE WHEN f.IsSuccess = 0 THEN 1 ELSE 0 END) AS FailedEvents,
        SUM(CASE WHEN f.EventType = 'UserAction' THEN 1 ELSE 0 END) AS UserActions,
        SUM(CASE WHEN f.EventType = 'ClaimChange' THEN 1 ELSE 0 END) AS ClaimChanges,
        SUM(CASE WHEN f.EventType = 'ApiActivity' THEN 1 ELSE 0 END) AS ApiActivities,
        COUNT(DISTINCT f.UserId) AS DistinctUsers,
        COUNT(DISTINCT f.ClaimId) AS DistinctClaims
    FROM Filtered f;
END;
GO
