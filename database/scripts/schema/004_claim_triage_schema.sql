IF COL_LENGTH('dbo.Claims', 'Priority') IS NULL
BEGIN
    ALTER TABLE dbo.Claims ADD Priority INT NOT NULL CONSTRAINT DF_Claims_Priority DEFAULT(2);
END;
GO

IF COL_LENGTH('dbo.Claims', 'WorkflowStep') IS NULL
BEGIN
    ALTER TABLE dbo.Claims ADD WorkflowStep NVARCHAR(100) NOT NULL CONSTRAINT DF_Claims_WorkflowStep DEFAULT('Registration');
END;
GO

IF COL_LENGTH('dbo.Claims', 'InvestigatorUserId') IS NULL
BEGIN
    ALTER TABLE dbo.Claims ADD InvestigatorUserId UNIQUEIDENTIFIER NULL;
END;
GO

IF COL_LENGTH('dbo.Claims', 'AdjusterUserId') IS NULL
BEGIN
    ALTER TABLE dbo.Claims ADD AdjusterUserId UNIQUEIDENTIFIER NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Claims_Users_Investigator')
BEGIN
    ALTER TABLE dbo.Claims
    ADD CONSTRAINT FK_Claims_Users_Investigator FOREIGN KEY (InvestigatorUserId) REFERENCES dbo.Users(UserId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Claims_Users_Adjuster')
BEGIN
    ALTER TABLE dbo.Claims
    ADD CONSTRAINT FK_Claims_Users_Adjuster FOREIGN KEY (AdjusterUserId) REFERENCES dbo.Users(UserId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Claims') AND name = 'IX_Claims_InvestigatorUserId')
BEGIN
    CREATE INDEX IX_Claims_InvestigatorUserId ON dbo.Claims(InvestigatorUserId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Claims') AND name = 'IX_Claims_AdjusterUserId')
BEGIN
    CREATE INDEX IX_Claims_AdjusterUserId ON dbo.Claims(AdjusterUserId);
END;
GO

IF OBJECT_ID('dbo.ClaimWorkflowHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClaimWorkflowHistory
    (
        ClaimWorkflowHistoryId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClaimId UNIQUEIDENTIFIER NOT NULL,
        ActionType NVARCHAR(100) NOT NULL,
        PreviousValue NVARCHAR(300) NULL,
        NewValue NVARCHAR(300) NOT NULL,
        ChangedByUserId UNIQUEIDENTIFIER NULL,
        ChangedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ClaimWorkflowHistory_ChangedAtUtc DEFAULT SYSUTCDATETIME(),
        Notes NVARCHAR(500) NULL,
        CONSTRAINT FK_ClaimWorkflowHistory_Claims FOREIGN KEY (ClaimId) REFERENCES dbo.Claims(ClaimId),
        CONSTRAINT FK_ClaimWorkflowHistory_Users FOREIGN KEY (ChangedByUserId) REFERENCES dbo.Users(UserId)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.ClaimWorkflowHistory') AND name = 'IX_ClaimWorkflowHistory_ClaimId')
BEGIN
    CREATE INDEX IX_ClaimWorkflowHistory_ClaimId ON dbo.ClaimWorkflowHistory(ClaimId, ChangedAtUtc DESC);
END;
GO
