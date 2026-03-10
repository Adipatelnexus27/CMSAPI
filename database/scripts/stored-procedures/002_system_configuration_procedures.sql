CREATE OR ALTER PROCEDURE dbo.sp_Config_GetLookupItems
    @ConfigType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ConfigurationItemId, ConfigType, Name, Code, Description, DisplayOrder, IsActive
    FROM dbo.ConfigurationItems
    WHERE ConfigType = @ConfigType
    ORDER BY DisplayOrder, Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_CreateLookupItem
    @ConfigType NVARCHAR(50),
    @Name NVARCHAR(200),
    @Code NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @DisplayOrder INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ConfigurationItemId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.ConfigurationItems
    (
        ConfigurationItemId,
        ConfigType,
        Name,
        Code,
        Description,
        DisplayOrder,
        IsActive
    )
    VALUES
    (
        @ConfigurationItemId,
        @ConfigType,
        @Name,
        @Code,
        @Description,
        @DisplayOrder,
        @IsActive
    );

    SELECT ConfigurationItemId, ConfigType, Name, Code, Description, DisplayOrder, IsActive
    FROM dbo.ConfigurationItems
    WHERE ConfigurationItemId = @ConfigurationItemId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_UpdateLookupItem
    @ConfigurationItemId UNIQUEIDENTIFIER,
    @ConfigType NVARCHAR(50),
    @Name NVARCHAR(200),
    @Code NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @DisplayOrder INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ConfigurationItems
    SET ConfigType = @ConfigType,
        Name = @Name,
        Code = @Code,
        Description = @Description,
        DisplayOrder = @DisplayOrder,
        IsActive = @IsActive,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE ConfigurationItemId = @ConfigurationItemId;

    SELECT ConfigurationItemId, ConfigType, Name, Code, Description, DisplayOrder, IsActive
    FROM dbo.ConfigurationItems
    WHERE ConfigurationItemId = @ConfigurationItemId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_DeleteLookupItem
    @ConfigurationItemId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.ConfigurationItems
    WHERE ConfigurationItemId = @ConfigurationItemId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_GetFraudRules
AS
BEGIN
    SET NOCOUNT ON;

    SELECT FraudRuleId, RuleName, RuleExpression, SeverityScore, Priority, IsActive
    FROM dbo.FraudDetectionRules
    ORDER BY Priority, RuleName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_CreateFraudRule
    @RuleName NVARCHAR(200),
    @RuleExpression NVARCHAR(MAX),
    @SeverityScore INT,
    @Priority INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FraudRuleId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.FraudDetectionRules
    (
        FraudRuleId,
        RuleName,
        RuleExpression,
        SeverityScore,
        Priority,
        IsActive
    )
    VALUES
    (
        @FraudRuleId,
        @RuleName,
        @RuleExpression,
        @SeverityScore,
        @Priority,
        @IsActive
    );

    SELECT FraudRuleId, RuleName, RuleExpression, SeverityScore, Priority, IsActive
    FROM dbo.FraudDetectionRules
    WHERE FraudRuleId = @FraudRuleId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_UpdateFraudRule
    @FraudRuleId UNIQUEIDENTIFIER,
    @RuleName NVARCHAR(200),
    @RuleExpression NVARCHAR(MAX),
    @SeverityScore INT,
    @Priority INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.FraudDetectionRules
    SET RuleName = @RuleName,
        RuleExpression = @RuleExpression,
        SeverityScore = @SeverityScore,
        Priority = @Priority,
        IsActive = @IsActive,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE FraudRuleId = @FraudRuleId;

    SELECT FraudRuleId, RuleName, RuleExpression, SeverityScore, Priority, IsActive
    FROM dbo.FraudDetectionRules
    WHERE FraudRuleId = @FraudRuleId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_DeleteFraudRule
    @FraudRuleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.FraudDetectionRules
    WHERE FraudRuleId = @FraudRuleId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_GetWorkflowSettings
AS
BEGIN
    SET NOCOUNT ON;

    SELECT WorkflowSettingId, WorkflowKey, StepName, StepSequence, AssignedRole, SlaHours, IsActive
    FROM dbo.WorkflowSettings
    ORDER BY WorkflowKey, StepSequence;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_CreateWorkflowSetting
    @WorkflowKey NVARCHAR(100),
    @StepName NVARCHAR(200),
    @StepSequence INT,
    @AssignedRole NVARCHAR(100),
    @SlaHours INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @WorkflowSettingId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.WorkflowSettings
    (
        WorkflowSettingId,
        WorkflowKey,
        StepName,
        StepSequence,
        AssignedRole,
        SlaHours,
        IsActive
    )
    VALUES
    (
        @WorkflowSettingId,
        @WorkflowKey,
        @StepName,
        @StepSequence,
        @AssignedRole,
        @SlaHours,
        @IsActive
    );

    SELECT WorkflowSettingId, WorkflowKey, StepName, StepSequence, AssignedRole, SlaHours, IsActive
    FROM dbo.WorkflowSettings
    WHERE WorkflowSettingId = @WorkflowSettingId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_UpdateWorkflowSetting
    @WorkflowSettingId UNIQUEIDENTIFIER,
    @WorkflowKey NVARCHAR(100),
    @StepName NVARCHAR(200),
    @StepSequence INT,
    @AssignedRole NVARCHAR(100),
    @SlaHours INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.WorkflowSettings
    SET WorkflowKey = @WorkflowKey,
        StepName = @StepName,
        StepSequence = @StepSequence,
        AssignedRole = @AssignedRole,
        SlaHours = @SlaHours,
        IsActive = @IsActive,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE WorkflowSettingId = @WorkflowSettingId;

    SELECT WorkflowSettingId, WorkflowKey, StepName, StepSequence, AssignedRole, SlaHours, IsActive
    FROM dbo.WorkflowSettings
    WHERE WorkflowSettingId = @WorkflowSettingId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Config_DeleteWorkflowSetting
    @WorkflowSettingId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.WorkflowSettings
    WHERE WorkflowSettingId = @WorkflowSettingId;
END;
GO
