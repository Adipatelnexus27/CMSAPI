/*
    Auth module DB update for CMS_ClaimManagement
    Server: (LocalDb)\MSSQLLocalDB
    Auth: Windows Authentication

    sqlcmd -S "(LocalDb)\MSSQLLocalDB" -E -i ".\database\CMS_Auth_Module_Update.sql"
*/

USE [CMS_ClaimManagement];
GO

IF COL_LENGTH('dbo.Mst_User', 'PasswordHash') IS NULL
    ALTER TABLE dbo.Mst_User ADD PasswordHash VARBINARY(64) NULL;
GO

IF COL_LENGTH('dbo.Mst_User', 'PasswordSalt') IS NULL
    ALTER TABLE dbo.Mst_User ADD PasswordSalt VARBINARY(128) NULL;
GO

IF COL_LENGTH('dbo.Mst_User', 'LastPasswordChangedDate') IS NULL
    ALTER TABLE dbo.Mst_User ADD LastPasswordChangedDate DATETIME2(0) NULL;
GO

IF OBJECT_ID('dbo.Mst_Permission', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Mst_Permission
    (
        PermissionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_Permission PRIMARY KEY,
        PermissionCode NVARCHAR(100) NOT NULL,
        PermissionName NVARCHAR(150) NOT NULL,
        PermissionDescription NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Mst_Permission_IsActive DEFAULT(1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_Permission_CreatedDate DEFAULT(SYSUTCDATETIME()),
        CreatedBy NVARCHAR(100) NOT NULL,
        ModifiedDate DATETIME2(0) NULL,
        ModifiedBy NVARCHAR(100) NULL
    );
END;
GO

IF OBJECT_ID('dbo.Mst_RolePermission', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Mst_RolePermission
    (
        RolePermissionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_RolePermission PRIMARY KEY,
        RoleId BIGINT NOT NULL,
        PermissionId BIGINT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Mst_RolePermission_IsActive DEFAULT(1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_RolePermission_CreatedDate DEFAULT(SYSUTCDATETIME()),
        CreatedBy NVARCHAR(100) NOT NULL,
        ModifiedDate DATETIME2(0) NULL,
        ModifiedBy NVARCHAR(100) NULL
    );
END;
GO

IF OBJECT_ID('dbo.Trn_UserRefreshToken', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Trn_UserRefreshToken
    (
        RefreshTokenId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_UserRefreshToken PRIMARY KEY,
        UserId BIGINT NOT NULL,
        TokenHash NVARCHAR(256) NOT NULL,
        ExpiresDate DATETIME2(0) NOT NULL,
        RevokedDate DATETIME2(0) NULL,
        ReplacedByTokenHash NVARCHAR(256) NULL,
        CreatedFromIp NVARCHAR(100) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Trn_UserRefreshToken_IsActive DEFAULT(1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_UserRefreshToken_CreatedDate DEFAULT(SYSUTCDATETIME()),
        CreatedBy NVARCHAR(100) NOT NULL,
        ModifiedDate DATETIME2(0) NULL,
        ModifiedBy NVARCHAR(100) NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Mst_Permission_Code' AND object_id = OBJECT_ID('dbo.Mst_Permission'))
    CREATE UNIQUE INDEX UX_Mst_Permission_Code ON dbo.Mst_Permission(PermissionCode);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Mst_RolePermission_Role_Permission' AND object_id = OBJECT_ID('dbo.Mst_RolePermission'))
    CREATE UNIQUE INDEX UX_Mst_RolePermission_Role_Permission ON dbo.Mst_RolePermission(RoleId, PermissionId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mst_RolePermission_RoleId' AND object_id = OBJECT_ID('dbo.Mst_RolePermission'))
    CREATE INDEX IX_Mst_RolePermission_RoleId ON dbo.Mst_RolePermission(RoleId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mst_RolePermission_PermissionId' AND object_id = OBJECT_ID('dbo.Mst_RolePermission'))
    CREATE INDEX IX_Mst_RolePermission_PermissionId ON dbo.Mst_RolePermission(PermissionId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Trn_UserRefreshToken_TokenHash' AND object_id = OBJECT_ID('dbo.Trn_UserRefreshToken'))
    CREATE UNIQUE INDEX UX_Trn_UserRefreshToken_TokenHash ON dbo.Trn_UserRefreshToken(TokenHash);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Trn_UserRefreshToken_UserId' AND object_id = OBJECT_ID('dbo.Trn_UserRefreshToken'))
    CREATE INDEX IX_Trn_UserRefreshToken_UserId ON dbo.Trn_UserRefreshToken(UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Trn_UserRefreshToken_ExpiresDate' AND object_id = OBJECT_ID('dbo.Trn_UserRefreshToken'))
    CREATE INDEX IX_Trn_UserRefreshToken_ExpiresDate ON dbo.Trn_UserRefreshToken(ExpiresDate);
GO

