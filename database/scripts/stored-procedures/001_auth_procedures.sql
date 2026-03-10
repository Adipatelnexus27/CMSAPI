CREATE OR ALTER PROCEDURE dbo.sp_Auth_RegisterUser
    @UserId UNIQUEIDENTIFIER,
    @Email NVARCHAR(320),
    @FullName NVARCHAR(200),
    @PasswordHash NVARCHAR(500),
    @PasswordSalt NVARCHAR(500),
    @RoleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Users(UserId, Email, FullName, PasswordHash, PasswordSalt, IsActive)
    VALUES(@UserId, @Email, @FullName, @PasswordHash, @PasswordSalt, 1);
    INSERT INTO dbo.UserRoles(UserRoleId, UserId, RoleId)
    SELECT NEWID(), @UserId, r.RoleId
    FROM dbo.Roles r
    WHERE r.Name = @RoleName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Auth_GetUserByEmail @Email NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 UserId, Email, FullName, PasswordHash, PasswordSalt, IsActive
    FROM dbo.Users
    WHERE Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Auth_GetUserById @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 UserId, Email, FullName, PasswordHash, PasswordSalt, IsActive
    FROM dbo.Users
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Auth_GetUserRoles @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.Name
    FROM dbo.UserRoles ur
    INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
    WHERE ur.UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Auth_GetUserPermissions @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT p.Name
    FROM dbo.UserRoles ur
    INNER JOIN dbo.RolePermissions rp ON rp.RoleId = ur.RoleId
    INNER JOIN dbo.Permissions p ON p.PermissionId = rp.PermissionId
    WHERE ur.UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Auth_StoreRefreshToken
    @RefreshTokenId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @TokenHash NVARCHAR(128),
    @ExpiresAtUtc DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.RefreshTokens(RefreshTokenId, UserId, TokenHash, ExpiresAtUtc)
    VALUES(@RefreshTokenId, @UserId, @TokenHash, @ExpiresAtUtc);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Auth_GetRefreshToken @TokenHash NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 RefreshTokenId, UserId, TokenHash, ExpiresAtUtc, RevokedAtUtc
    FROM dbo.RefreshTokens
    WHERE TokenHash = @TokenHash;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Auth_RevokeRefreshToken
    @TokenHash NVARCHAR(128),
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.RefreshTokens
    SET RevokedAtUtc = COALESCE(RevokedAtUtc, SYSUTCDATETIME()),
        RevokedReason = @Reason
    WHERE TokenHash = @TokenHash;
END;
GO

