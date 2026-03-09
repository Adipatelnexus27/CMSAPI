/*
    Policy module data seed/update for CMS_ClaimManagement
    Server: (LocalDb)\MSSQLLocalDB
    Auth: Windows Authentication

    sqlcmd -S "(LocalDb)\MSSQLLocalDB" -E -i ".\database\CMS_Policy_Module_Update.sql"
*/

USE [CMS_ClaimManagement];
GO

IF OBJECT_ID('dbo.Mst_PolicyType','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Mst_PolicyType WHERE PolicyTypeCode = 'GEN')
BEGIN
    INSERT INTO dbo.Mst_PolicyType (PolicyTypeCode, PolicyTypeName, PolicyTypeDescription, IsActive, CreatedDate, CreatedBy)
    VALUES ('GEN', 'General Insurance', 'General policy', 1, SYSUTCDATETIME(), 'seed');
END;
GO

IF OBJECT_ID('dbo.Mst_Currency','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Mst_Currency WHERE CurrencyCode = 'USD')
BEGIN
    INSERT INTO dbo.Mst_Currency (CurrencyCode, CurrencyName, CurrencySymbol, IsActive, CreatedDate, CreatedBy)
    VALUES ('USD', 'US Dollar', '$', 1, SYSUTCDATETIME(), 'seed');
END;
GO

IF OBJECT_ID('dbo.Mst_CoverageType','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Mst_CoverageType WHERE CoverageCode = 'OD')
        INSERT INTO dbo.Mst_CoverageType (CoverageCode, CoverageName, CoverageDescription, IsActive, CreatedDate, CreatedBy)
        VALUES ('OD', 'Own Damage', 'Own damage coverage', 1, SYSUTCDATETIME(), 'seed');

    IF NOT EXISTS (SELECT 1 FROM dbo.Mst_CoverageType WHERE CoverageCode = 'TPL')
        INSERT INTO dbo.Mst_CoverageType (CoverageCode, CoverageName, CoverageDescription, IsActive, CreatedDate, CreatedBy)
        VALUES ('TPL', 'Third Party Liability', 'Third party liability coverage', 1, SYSUTCDATETIME(), 'seed');
END;
GO

DECLARE @PolicyTypeId BIGINT = (SELECT TOP 1 PolicyTypeId FROM dbo.Mst_PolicyType WHERE PolicyTypeCode = 'GEN');
DECLARE @CurrencyId BIGINT = (SELECT TOP 1 CurrencyId FROM dbo.Mst_Currency WHERE CurrencyCode = 'USD');

IF @PolicyTypeId IS NOT NULL AND @CurrencyId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Mst_Policy WHERE PolicyNumber = 'POL-10001')
    BEGIN
        INSERT INTO dbo.Mst_Policy
        (
            PolicyNumber, PolicyTypeId, InsuredName, PolicyStartDate, PolicyEndDate,
            SumInsured, CurrencyId, PolicyStatus, IsActive, CreatedDate, CreatedBy
        )
        VALUES
        (
            'POL-10001', @PolicyTypeId, 'Default Insured', DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE)), DATEADD(DAY, 335, CAST(GETUTCDATE() AS DATE)),
            100000.00, @CurrencyId, 'Active', 1, SYSUTCDATETIME(), 'seed'
        );
    END;
END;
GO

DECLARE @PolicyId BIGINT = (SELECT TOP 1 PolicyId FROM dbo.Mst_Policy WHERE PolicyNumber = 'POL-10001');
DECLARE @CoverageOD BIGINT = (SELECT TOP 1 CoverageTypeId FROM dbo.Mst_CoverageType WHERE CoverageCode = 'OD');
DECLARE @CoverageTPL BIGINT = (SELECT TOP 1 CoverageTypeId FROM dbo.Mst_CoverageType WHERE CoverageCode = 'TPL');

IF @PolicyId IS NOT NULL AND @CoverageOD IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Mst_PolicyCoverage WHERE PolicyId = @PolicyId AND CoverageTypeId = @CoverageOD)
    BEGIN
        INSERT INTO dbo.Mst_PolicyCoverage
        (
            PolicyId, CoverageTypeId, CoverageLimit, DeductibleAmount, EffectiveFrom, EffectiveTo,
            IsActive, CreatedDate, CreatedBy
        )
        VALUES
        (
            @PolicyId, @CoverageOD, 75000.00, 1000.00, DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE)), DATEADD(DAY, 335, CAST(GETUTCDATE() AS DATE)),
            1, SYSUTCDATETIME(), 'seed'
        );
    END;
END;
GO

IF @PolicyId IS NOT NULL AND @CoverageTPL IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Mst_PolicyCoverage WHERE PolicyId = @PolicyId AND CoverageTypeId = @CoverageTPL)
    BEGIN
        INSERT INTO dbo.Mst_PolicyCoverage
        (
            PolicyId, CoverageTypeId, CoverageLimit, DeductibleAmount, EffectiveFrom, EffectiveTo,
            IsActive, CreatedDate, CreatedBy
        )
        VALUES
        (
            @PolicyId, @CoverageTPL, 50000.00, 500.00, DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE)), DATEADD(DAY, 335, CAST(GETUTCDATE() AS DATE)),
            1, SYSUTCDATETIME(), 'seed'
        );
    END;
END;
GO

