IF OBJECT_ID('dbo.ConfigurationItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConfigurationItems
    (
        ConfigurationItemId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ConfigType NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL CONSTRAINT DF_ConfigurationItems_DisplayOrder DEFAULT(0),
        IsActive BIT NOT NULL CONSTRAINT DF_ConfigurationItems_IsActive DEFAULT(1),
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ConfigurationItems_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        CONSTRAINT CK_ConfigurationItems_ConfigType CHECK (ConfigType IN ('InsuranceProduct','PolicyType','ClaimType','ClaimStatus'))
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.ConfigurationItems') AND name = 'UQ_ConfigurationItems_ConfigType_Name')
BEGIN
    CREATE UNIQUE INDEX UQ_ConfigurationItems_ConfigType_Name ON dbo.ConfigurationItems(ConfigType, Name);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.ConfigurationItems') AND name = 'UQ_ConfigurationItems_ConfigType_Code')
BEGIN
    CREATE UNIQUE INDEX UQ_ConfigurationItems_ConfigType_Code ON dbo.ConfigurationItems(ConfigType, Code);
END;
GO

IF OBJECT_ID('dbo.FraudDetectionRules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FraudDetectionRules
    (
        FraudRuleId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        RuleName NVARCHAR(200) NOT NULL,
        RuleExpression NVARCHAR(MAX) NOT NULL,
        SeverityScore INT NOT NULL,
        Priority INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_FraudDetectionRules_IsActive DEFAULT(1),
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_FraudDetectionRules_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        CONSTRAINT CK_FraudDetectionRules_SeverityScore CHECK (SeverityScore >= 0 AND SeverityScore <= 100),
        CONSTRAINT CK_FraudDetectionRules_Priority CHECK (Priority >= 0)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.FraudDetectionRules') AND name = 'UQ_FraudDetectionRules_RuleName')
BEGIN
    CREATE UNIQUE INDEX UQ_FraudDetectionRules_RuleName ON dbo.FraudDetectionRules(RuleName);
END;
GO

IF OBJECT_ID('dbo.WorkflowSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkflowSettings
    (
        WorkflowSettingId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        WorkflowKey NVARCHAR(100) NOT NULL,
        StepName NVARCHAR(200) NOT NULL,
        StepSequence INT NOT NULL,
        AssignedRole NVARCHAR(100) NOT NULL,
        SlaHours INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_WorkflowSettings_IsActive DEFAULT(1),
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_WorkflowSettings_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        CONSTRAINT CK_WorkflowSettings_StepSequence CHECK (StepSequence > 0),
        CONSTRAINT CK_WorkflowSettings_SlaHours CHECK (SlaHours >= 0)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.WorkflowSettings') AND name = 'UQ_WorkflowSettings_WorkflowKey_Sequence')
BEGIN
    CREATE UNIQUE INDEX UQ_WorkflowSettings_WorkflowKey_Sequence ON dbo.WorkflowSettings(WorkflowKey, StepSequence);
END;
GO
