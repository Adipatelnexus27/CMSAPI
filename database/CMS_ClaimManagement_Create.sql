/*
    Claim Management System Database Script
    SQL Server : (LocalDb)\MSSQLLocalDB
    Authentication: Windows Authentication

    Example:
    sqlcmd -S "(LocalDb)\MSSQLLocalDB" -E -i ".\database\CMS_ClaimManagement_Create.sql"

    Note:
    - No hard foreign key constraints are created at DB level (as requested).
    - PK/Unique constraints and performance indexes are included.
*/

IF DB_ID(N'CMS_ClaimManagement') IS NULL
BEGIN
    CREATE DATABASE [CMS_ClaimManagement];
END
GO

USE [CMS_ClaimManagement];
GO

/* ===============================
   MASTER TABLES (Mst_)
   =============================== */

CREATE TABLE dbo.Mst_UserRole
(
    RoleId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_UserRole PRIMARY KEY,
    RoleCode NVARCHAR(50) NOT NULL,
    RoleName NVARCHAR(100) NOT NULL,
    RoleDescription NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_UserRole_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_UserRole_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_UserRole_RoleCode ON dbo.Mst_UserRole(RoleCode);
GO

CREATE TABLE dbo.Mst_Department
(
    DepartmentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_Department PRIMARY KEY,
    DepartmentCode NVARCHAR(50) NOT NULL,
    DepartmentName NVARCHAR(150) NOT NULL,
    DepartmentDescription NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_Department_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_Department_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_Department_DepartmentCode ON dbo.Mst_Department(DepartmentCode);
GO

CREATE TABLE dbo.Mst_User
(
    UserId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_User PRIMARY KEY,
    UserName NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    MobileNo NVARCHAR(30) NULL,
    RoleId BIGINT NOT NULL,
    DepartmentId BIGINT NULL,
    IsLocked BIT NOT NULL CONSTRAINT DF_Mst_User_IsLocked DEFAULT (0),
    LastLoginDate DATETIME2(0) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_User_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_User_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_User_UserName ON dbo.Mst_User(UserName);
CREATE UNIQUE INDEX UX_Mst_User_Email ON dbo.Mst_User(Email);
CREATE INDEX IX_Mst_User_RoleId ON dbo.Mst_User(RoleId);
CREATE INDEX IX_Mst_User_DepartmentId ON dbo.Mst_User(DepartmentId);
GO

CREATE TABLE dbo.Mst_ClaimType
(
    ClaimTypeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_ClaimType PRIMARY KEY,
    ClaimTypeCode NVARCHAR(50) NOT NULL,
    ClaimTypeName NVARCHAR(120) NOT NULL,
    ClaimTypeDescription NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_ClaimType_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_ClaimType_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_ClaimType_ClaimTypeCode ON dbo.Mst_ClaimType(ClaimTypeCode);
GO

CREATE TABLE dbo.Mst_ClaimStatus
(
    ClaimStatusId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_ClaimStatus PRIMARY KEY,
    StatusCode NVARCHAR(50) NOT NULL,
    StatusName NVARCHAR(120) NOT NULL,
    SequenceNo INT NOT NULL,
    IsTerminalStatus BIT NOT NULL CONSTRAINT DF_Mst_ClaimStatus_IsTerminalStatus DEFAULT (0),
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_ClaimStatus_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_ClaimStatus_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_ClaimStatus_StatusCode ON dbo.Mst_ClaimStatus(StatusCode);
CREATE INDEX IX_Mst_ClaimStatus_SequenceNo ON dbo.Mst_ClaimStatus(SequenceNo);
GO

CREATE TABLE dbo.Mst_PolicyType
(
    PolicyTypeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_PolicyType PRIMARY KEY,
    PolicyTypeCode NVARCHAR(50) NOT NULL,
    PolicyTypeName NVARCHAR(120) NOT NULL,
    PolicyTypeDescription NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_PolicyType_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_PolicyType_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_PolicyType_PolicyTypeCode ON dbo.Mst_PolicyType(PolicyTypeCode);
GO

CREATE TABLE dbo.Mst_Currency
(
    CurrencyId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_Currency PRIMARY KEY,
    CurrencyCode NVARCHAR(10) NOT NULL,
    CurrencyName NVARCHAR(80) NOT NULL,
    CurrencySymbol NVARCHAR(10) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_Currency_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_Currency_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_Currency_CurrencyCode ON dbo.Mst_Currency(CurrencyCode);
GO

CREATE TABLE dbo.Mst_Policy
(
    PolicyId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_Policy PRIMARY KEY,
    PolicyNumber NVARCHAR(50) NOT NULL,
    PolicyTypeId BIGINT NOT NULL,
    InsuredName NVARCHAR(200) NOT NULL,
    PolicyStartDate DATE NOT NULL,
    PolicyEndDate DATE NOT NULL,
    SumInsured DECIMAL(18,2) NOT NULL,
    CurrencyId BIGINT NOT NULL,
    PolicyStatus NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_Policy_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_Policy_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_Policy_PolicyNumber ON dbo.Mst_Policy(PolicyNumber);
CREATE INDEX IX_Mst_Policy_PolicyTypeId ON dbo.Mst_Policy(PolicyTypeId);
CREATE INDEX IX_Mst_Policy_CurrencyId ON dbo.Mst_Policy(CurrencyId);
CREATE INDEX IX_Mst_Policy_PolicyEndDate ON dbo.Mst_Policy(PolicyEndDate);
GO

CREATE TABLE dbo.Mst_CoverageType
(
    CoverageTypeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_CoverageType PRIMARY KEY,
    CoverageCode NVARCHAR(50) NOT NULL,
    CoverageName NVARCHAR(150) NOT NULL,
    CoverageDescription NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_CoverageType_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_CoverageType_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_CoverageType_CoverageCode ON dbo.Mst_CoverageType(CoverageCode);
GO

CREATE TABLE dbo.Mst_PolicyCoverage
(
    PolicyCoverageId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_PolicyCoverage PRIMARY KEY,
    PolicyId BIGINT NOT NULL,
    CoverageTypeId BIGINT NOT NULL,
    CoverageLimit DECIMAL(18,2) NOT NULL,
    DeductibleAmount DECIMAL(18,2) NULL,
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_PolicyCoverage_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_PolicyCoverage_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Mst_PolicyCoverage_PolicyId ON dbo.Mst_PolicyCoverage(PolicyId);
CREATE INDEX IX_Mst_PolicyCoverage_CoverageTypeId ON dbo.Mst_PolicyCoverage(CoverageTypeId);
GO

CREATE TABLE dbo.Mst_DocumentType
(
    DocumentTypeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_DocumentType PRIMARY KEY,
    DocumentTypeCode NVARCHAR(50) NOT NULL,
    DocumentTypeName NVARCHAR(120) NOT NULL,
    IsMandatory BIT NOT NULL CONSTRAINT DF_Mst_DocumentType_IsMandatory DEFAULT (0),
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_DocumentType_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_DocumentType_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_DocumentType_DocumentTypeCode ON dbo.Mst_DocumentType(DocumentTypeCode);
GO

CREATE TABLE dbo.Mst_InvestigationType
(
    InvestigationTypeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_InvestigationType PRIMARY KEY,
    InvestigationTypeCode NVARCHAR(50) NOT NULL,
    InvestigationTypeName NVARCHAR(120) NOT NULL,
    InvestigationTypeDescription NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_InvestigationType_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_InvestigationType_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_InvestigationType_Code ON dbo.Mst_InvestigationType(InvestigationTypeCode);
GO

CREATE TABLE dbo.Mst_FraudRule
(
    FraudRuleId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_FraudRule PRIMARY KEY,
    RuleCode NVARCHAR(50) NOT NULL,
    RuleName NVARCHAR(150) NOT NULL,
    RuleWeight DECIMAL(5,2) NOT NULL,
    RuleDefinition NVARCHAR(2000) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_FraudRule_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_FraudRule_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_FraudRule_RuleCode ON dbo.Mst_FraudRule(RuleCode);
GO

CREATE TABLE dbo.Mst_PaymentMethod
(
    PaymentMethodId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_PaymentMethod PRIMARY KEY,
    PaymentMethodCode NVARCHAR(50) NOT NULL,
    PaymentMethodName NVARCHAR(120) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_PaymentMethod_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_PaymentMethod_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_PaymentMethod_Code ON dbo.Mst_PaymentMethod(PaymentMethodCode);
GO

CREATE TABLE dbo.Mst_NotificationChannel
(
    NotificationChannelId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_NotificationChannel PRIMARY KEY,
    ChannelCode NVARCHAR(50) NOT NULL,
    ChannelName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_NotificationChannel_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_NotificationChannel_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_NotificationChannel_Code ON dbo.Mst_NotificationChannel(ChannelCode);
GO

CREATE TABLE dbo.Mst_WorkflowDefinition
(
    WorkflowDefinitionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_WorkflowDefinition PRIMARY KEY,
    WorkflowCode NVARCHAR(50) NOT NULL,
    WorkflowName NVARCHAR(150) NOT NULL,
    WorkflowDescription NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_WorkflowDefinition_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_WorkflowDefinition_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Mst_WorkflowDefinition_Code ON dbo.Mst_WorkflowDefinition(WorkflowCode);
GO

CREATE TABLE dbo.Mst_WorkflowStage
(
    WorkflowStageId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mst_WorkflowStage PRIMARY KEY,
    WorkflowDefinitionId BIGINT NOT NULL,
    StageCode NVARCHAR(50) NOT NULL,
    StageName NVARCHAR(150) NOT NULL,
    StageSequence INT NOT NULL,
    SLAInHours INT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Mst_WorkflowStage_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Mst_WorkflowStage_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Mst_WorkflowStage_WorkflowDefinitionId ON dbo.Mst_WorkflowStage(WorkflowDefinitionId);
CREATE UNIQUE INDEX UX_Mst_WorkflowStage_StageCode ON dbo.Mst_WorkflowStage(StageCode);
GO

/* ===============================
   TRANSACTION TABLES (Trn_)
   =============================== */

CREATE TABLE dbo.Trn_Claim
(
    ClaimId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_Claim PRIMARY KEY,
    ClaimNumber NVARCHAR(50) NOT NULL,
    PolicyId BIGINT NOT NULL,
    ClaimTypeId BIGINT NOT NULL,
    CurrentStatusId BIGINT NOT NULL,
    LossDate DATETIME2(0) NOT NULL,
    ReportedDate DATETIME2(0) NOT NULL,
    IncidentDescription NVARCHAR(2000) NULL,
    LocationOfLoss NVARCHAR(500) NULL,
    EstimatedLossAmount DECIMAL(18,2) NOT NULL,
    ApprovedLossAmount DECIMAL(18,2) NULL,
    CurrencyId BIGINT NOT NULL,
    FraudScore DECIMAL(5,2) NULL,
    IsFraudSuspected BIT NOT NULL CONSTRAINT DF_Trn_Claim_IsFraudSuspected DEFAULT (0),
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_Claim_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_Claim_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Trn_Claim_ClaimNumber ON dbo.Trn_Claim(ClaimNumber);
CREATE INDEX IX_Trn_Claim_PolicyId ON dbo.Trn_Claim(PolicyId);
CREATE INDEX IX_Trn_Claim_ClaimTypeId ON dbo.Trn_Claim(ClaimTypeId);
CREATE INDEX IX_Trn_Claim_CurrentStatusId ON dbo.Trn_Claim(CurrentStatusId);
CREATE INDEX IX_Trn_Claim_ReportedDate ON dbo.Trn_Claim(ReportedDate);
CREATE INDEX IX_Trn_Claim_LossDate ON dbo.Trn_Claim(LossDate);
GO

CREATE TABLE dbo.Trn_ClaimParty
(
    ClaimPartyId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimParty PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    PartyType NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    ContactNo NVARCHAR(30) NULL,
    Email NVARCHAR(256) NULL,
    AddressLine NVARCHAR(300) NULL,
    City NVARCHAR(100) NULL,
    [State] NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimParty_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimParty_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimParty_ClaimId ON dbo.Trn_ClaimParty(ClaimId);
CREATE INDEX IX_Trn_ClaimParty_PartyType ON dbo.Trn_ClaimParty(PartyType);
GO

CREATE TABLE dbo.Trn_ClaimPolicyValidation
(
    PolicyValidationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimPolicyValidation PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    PolicyId BIGINT NOT NULL,
    ValidationDate DATETIME2(0) NOT NULL,
    IsPolicyValid BIT NOT NULL,
    ValidationResult NVARCHAR(1000) NULL,
    ValidatedByUserId BIGINT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimPolicyValidation_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimPolicyValidation_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimPolicyValidation_ClaimId ON dbo.Trn_ClaimPolicyValidation(ClaimId);
CREATE INDEX IX_Trn_ClaimPolicyValidation_PolicyId ON dbo.Trn_ClaimPolicyValidation(PolicyId);
CREATE INDEX IX_Trn_ClaimPolicyValidation_ValidationDate ON dbo.Trn_ClaimPolicyValidation(ValidationDate);
GO

CREATE TABLE dbo.Trn_ClaimAssignment
(
    ClaimAssignmentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimAssignment PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    AssignedToUserId BIGINT NOT NULL,
    AssignedByUserId BIGINT NOT NULL,
    AssignmentDate DATETIME2(0) NOT NULL,
    AssignmentReason NVARCHAR(500) NULL,
    IsCurrent BIT NOT NULL CONSTRAINT DF_Trn_ClaimAssignment_IsCurrent DEFAULT (1),
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimAssignment_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimAssignment_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimAssignment_ClaimId ON dbo.Trn_ClaimAssignment(ClaimId);
CREATE INDEX IX_Trn_ClaimAssignment_AssignedToUserId ON dbo.Trn_ClaimAssignment(AssignedToUserId);
CREATE INDEX IX_Trn_ClaimAssignment_IsCurrent ON dbo.Trn_ClaimAssignment(IsCurrent);
GO

CREATE TABLE dbo.Trn_ClaimStatusHistory
(
    ClaimStatusHistoryId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimStatusHistory PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    PreviousStatusId BIGINT NULL,
    NewStatusId BIGINT NOT NULL,
    ChangedDate DATETIME2(0) NOT NULL,
    ChangedByUserId BIGINT NOT NULL,
    Remarks NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimStatusHistory_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimStatusHistory_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimStatusHistory_ClaimId ON dbo.Trn_ClaimStatusHistory(ClaimId);
CREATE INDEX IX_Trn_ClaimStatusHistory_ChangedDate ON dbo.Trn_ClaimStatusHistory(ChangedDate);
GO

CREATE TABLE dbo.Trn_ClaimInvestigation
(
    InvestigationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimInvestigation PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    InvestigationTypeId BIGINT NOT NULL,
    InvestigatorUserId BIGINT NOT NULL,
    StartDate DATETIME2(0) NOT NULL,
    EndDate DATETIME2(0) NULL,
    FindingsSummary NVARCHAR(4000) NULL,
    Recommendation NVARCHAR(2000) NULL,
    InvestigationStatus NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimInvestigation_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimInvestigation_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimInvestigation_ClaimId ON dbo.Trn_ClaimInvestigation(ClaimId);
CREATE INDEX IX_Trn_ClaimInvestigation_InvestigatorUserId ON dbo.Trn_ClaimInvestigation(InvestigatorUserId);
CREATE INDEX IX_Trn_ClaimInvestigation_StartDate ON dbo.Trn_ClaimInvestigation(StartDate);
GO

CREATE TABLE dbo.Trn_ClaimInvestigationFinding
(
    InvestigationFindingId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimInvestigationFinding PRIMARY KEY,
    InvestigationId BIGINT NOT NULL,
    FindingTitle NVARCHAR(300) NOT NULL,
    FindingDetail NVARCHAR(3000) NULL,
    Severity NVARCHAR(30) NULL,
    ReportedOn DATETIME2(0) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimInvestigationFinding_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimInvestigationFinding_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimInvestigationFinding_InvestigationId ON dbo.Trn_ClaimInvestigationFinding(InvestigationId);
GO

CREATE TABLE dbo.Trn_ClaimFraudAssessment
(
    FraudAssessmentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimFraudAssessment PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    FraudRuleId BIGINT NULL,
    AssessmentDate DATETIME2(0) NOT NULL,
    Score DECIMAL(5,2) NOT NULL,
    RiskLevel NVARCHAR(30) NOT NULL,
    AssessmentResult NVARCHAR(1000) NULL,
    ReviewedByUserId BIGINT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimFraudAssessment_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimFraudAssessment_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimFraudAssessment_ClaimId ON dbo.Trn_ClaimFraudAssessment(ClaimId);
CREATE INDEX IX_Trn_ClaimFraudAssessment_AssessmentDate ON dbo.Trn_ClaimFraudAssessment(AssessmentDate);
CREATE INDEX IX_Trn_ClaimFraudAssessment_RiskLevel ON dbo.Trn_ClaimFraudAssessment(RiskLevel);
GO

CREATE TABLE dbo.Trn_ClaimCoverageDecision
(
    CoverageDecisionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimCoverageDecision PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    PolicyCoverageId BIGINT NULL,
    DecisionDate DATETIME2(0) NOT NULL,
    IsCovered BIT NOT NULL,
    CoveragePercent DECIMAL(5,2) NULL,
    ApprovedAmount DECIMAL(18,2) NULL,
    DecisionReason NVARCHAR(2000) NULL,
    DecidedByUserId BIGINT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimCoverageDecision_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimCoverageDecision_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimCoverageDecision_ClaimId ON dbo.Trn_ClaimCoverageDecision(ClaimId);
CREATE INDEX IX_Trn_ClaimCoverageDecision_DecisionDate ON dbo.Trn_ClaimCoverageDecision(DecisionDate);
GO

CREATE TABLE dbo.Trn_ClaimLiabilityDecision
(
    LiabilityDecisionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimLiabilityDecision PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    DecisionDate DATETIME2(0) NOT NULL,
    LiabilityPercent DECIMAL(5,2) NOT NULL,
    ThirdPartyInvolved BIT NOT NULL CONSTRAINT DF_Trn_ClaimLiabilityDecision_ThirdPartyInvolved DEFAULT (0),
    ThirdPartyName NVARCHAR(200) NULL,
    DecisionReason NVARCHAR(2000) NULL,
    DecidedByUserId BIGINT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimLiabilityDecision_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimLiabilityDecision_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimLiabilityDecision_ClaimId ON dbo.Trn_ClaimLiabilityDecision(ClaimId);
GO

CREATE TABLE dbo.Trn_ClaimReserve
(
    ReserveId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimReserve PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    ReserveType NVARCHAR(50) NOT NULL,
    ReserveAmount DECIMAL(18,2) NOT NULL,
    ReserveDate DATETIME2(0) NOT NULL,
    ReserveReason NVARCHAR(1000) NULL,
    ApprovedByUserId BIGINT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimReserve_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimReserve_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimReserve_ClaimId ON dbo.Trn_ClaimReserve(ClaimId);
CREATE INDEX IX_Trn_ClaimReserve_ReserveDate ON dbo.Trn_ClaimReserve(ReserveDate);
GO

CREATE TABLE dbo.Trn_ClaimSettlement
(
    SettlementId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimSettlement PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    SettlementDate DATETIME2(0) NOT NULL,
    SettlementAmount DECIMAL(18,2) NOT NULL,
    SettlementType NVARCHAR(50) NOT NULL,
    SettlementStatus NVARCHAR(50) NOT NULL,
    NegotiationNotes NVARCHAR(2000) NULL,
    ApprovedByUserId BIGINT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimSettlement_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimSettlement_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimSettlement_ClaimId ON dbo.Trn_ClaimSettlement(ClaimId);
CREATE INDEX IX_Trn_ClaimSettlement_SettlementDate ON dbo.Trn_ClaimSettlement(SettlementDate);
GO

CREATE TABLE dbo.Trn_ClaimPayment
(
    PaymentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimPayment PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    SettlementId BIGINT NULL,
    PaymentMethodId BIGINT NOT NULL,
    PaymentReferenceNo NVARCHAR(100) NOT NULL,
    PaymentDate DATETIME2(0) NOT NULL,
    PaymentAmount DECIMAL(18,2) NOT NULL,
    PaymentStatus NVARCHAR(50) NOT NULL,
    PaidTo NVARCHAR(200) NOT NULL,
    ProcessedByUserId BIGINT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimPayment_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimPayment_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE UNIQUE INDEX UX_Trn_ClaimPayment_PaymentReferenceNo ON dbo.Trn_ClaimPayment(PaymentReferenceNo);
CREATE INDEX IX_Trn_ClaimPayment_ClaimId ON dbo.Trn_ClaimPayment(ClaimId);
CREATE INDEX IX_Trn_ClaimPayment_SettlementId ON dbo.Trn_ClaimPayment(SettlementId);
CREATE INDEX IX_Trn_ClaimPayment_PaymentDate ON dbo.Trn_ClaimPayment(PaymentDate);
CREATE INDEX IX_Trn_ClaimPayment_PaymentStatus ON dbo.Trn_ClaimPayment(PaymentStatus);
GO

CREATE TABLE dbo.Trn_ClaimPaymentReconciliation
(
    ReconciliationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimPaymentReconciliation PRIMARY KEY,
    PaymentId BIGINT NOT NULL,
    ReconciliationDate DATETIME2(0) NOT NULL,
    BankReferenceNo NVARCHAR(100) NULL,
    IsReconciled BIT NOT NULL,
    ReconciliationNotes NVARCHAR(1000) NULL,
    ReconciledByUserId BIGINT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimPaymentReconciliation_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimPaymentReconciliation_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimPaymentReconciliation_PaymentId ON dbo.Trn_ClaimPaymentReconciliation(PaymentId);
CREATE INDEX IX_Trn_ClaimPaymentReconciliation_ReconciliationDate ON dbo.Trn_ClaimPaymentReconciliation(ReconciliationDate);
GO

CREATE TABLE dbo.Trn_ClaimDocument
(
    ClaimDocumentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimDocument PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    DocumentTypeId BIGINT NOT NULL,
    FileName NVARCHAR(260) NOT NULL,
    FilePath NVARCHAR(1000) NOT NULL,
    FileHash NVARCHAR(200) NULL,
    UploadedDate DATETIME2(0) NOT NULL,
    UploadedByUserId BIGINT NOT NULL,
    VersionNo INT NOT NULL CONSTRAINT DF_Trn_ClaimDocument_VersionNo DEFAULT (1),
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimDocument_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimDocument_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimDocument_ClaimId ON dbo.Trn_ClaimDocument(ClaimId);
CREATE INDEX IX_Trn_ClaimDocument_DocumentTypeId ON dbo.Trn_ClaimDocument(DocumentTypeId);
CREATE INDEX IX_Trn_ClaimDocument_UploadedDate ON dbo.Trn_ClaimDocument(UploadedDate);
GO

CREATE TABLE dbo.Trn_ClaimNote
(
    ClaimNoteId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimNote PRIMARY KEY,
    ClaimId BIGINT NOT NULL,
    NoteCategory NVARCHAR(50) NOT NULL,
    NoteText NVARCHAR(4000) NOT NULL,
    NotedDate DATETIME2(0) NOT NULL,
    NotedByUserId BIGINT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimNote_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimNote_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimNote_ClaimId ON dbo.Trn_ClaimNote(ClaimId);
CREATE INDEX IX_Trn_ClaimNote_NotedDate ON dbo.Trn_ClaimNote(NotedDate);
GO

CREATE TABLE dbo.Trn_AuditTrail
(
    AuditTrailId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_AuditTrail PRIMARY KEY,
    EntityName NVARCHAR(128) NOT NULL,
    EntityId BIGINT NOT NULL,
    ActionType NVARCHAR(50) NOT NULL,
    ActionDate DATETIME2(0) NOT NULL,
    ActionByUserId BIGINT NOT NULL,
    BeforeData NVARCHAR(MAX) NULL,
    AfterData NVARCHAR(MAX) NULL,
    CorrelationId NVARCHAR(100) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_AuditTrail_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_AuditTrail_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_AuditTrail_EntityName_EntityId ON dbo.Trn_AuditTrail(EntityName, EntityId);
CREATE INDEX IX_Trn_AuditTrail_ActionDate ON dbo.Trn_AuditTrail(ActionDate);
CREATE INDEX IX_Trn_AuditTrail_CorrelationId ON dbo.Trn_AuditTrail(CorrelationId);
GO

CREATE TABLE dbo.Trn_Notification
(
    NotificationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_Notification PRIMARY KEY,
    ClaimId BIGINT NULL,
    NotificationChannelId BIGINT NOT NULL,
    Recipient NVARCHAR(256) NOT NULL,
    [Subject] NVARCHAR(300) NULL,
    MessageBody NVARCHAR(MAX) NOT NULL,
    SentDate DATETIME2(0) NULL,
    NotificationStatus NVARCHAR(50) NOT NULL,
    ErrorMessage NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_Notification_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_Notification_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_Notification_ClaimId ON dbo.Trn_Notification(ClaimId);
CREATE INDEX IX_Trn_Notification_ChannelId ON dbo.Trn_Notification(NotificationChannelId);
CREATE INDEX IX_Trn_Notification_Status ON dbo.Trn_Notification(NotificationStatus);
CREATE INDEX IX_Trn_Notification_CreatedDate ON dbo.Trn_Notification(CreatedDate);
GO

CREATE TABLE dbo.Trn_WorkflowInstance
(
    WorkflowInstanceId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_WorkflowInstance PRIMARY KEY,
    WorkflowDefinitionId BIGINT NOT NULL,
    ClaimId BIGINT NOT NULL,
    CurrentStageId BIGINT NOT NULL,
    InstanceStatus NVARCHAR(50) NOT NULL,
    StartedDate DATETIME2(0) NOT NULL,
    CompletedDate DATETIME2(0) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_WorkflowInstance_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_WorkflowInstance_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_WorkflowInstance_ClaimId ON dbo.Trn_WorkflowInstance(ClaimId);
CREATE INDEX IX_Trn_WorkflowInstance_CurrentStageId ON dbo.Trn_WorkflowInstance(CurrentStageId);
CREATE INDEX IX_Trn_WorkflowInstance_InstanceStatus ON dbo.Trn_WorkflowInstance(InstanceStatus);
GO

CREATE TABLE dbo.Trn_WorkflowTask
(
    WorkflowTaskId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_WorkflowTask PRIMARY KEY,
    WorkflowInstanceId BIGINT NOT NULL,
    WorkflowStageId BIGINT NOT NULL,
    TaskName NVARCHAR(200) NOT NULL,
    AssignedToUserId BIGINT NULL,
    DueDate DATETIME2(0) NULL,
    CompletedDate DATETIME2(0) NULL,
    TaskStatus NVARCHAR(50) NOT NULL,
    TaskRemarks NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_WorkflowTask_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_WorkflowTask_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_WorkflowTask_WorkflowInstanceId ON dbo.Trn_WorkflowTask(WorkflowInstanceId);
CREATE INDEX IX_Trn_WorkflowTask_AssignedToUserId ON dbo.Trn_WorkflowTask(AssignedToUserId);
CREATE INDEX IX_Trn_WorkflowTask_TaskStatus ON dbo.Trn_WorkflowTask(TaskStatus);
CREATE INDEX IX_Trn_WorkflowTask_DueDate ON dbo.Trn_WorkflowTask(DueDate);
GO

CREATE TABLE dbo.Trn_ReportExecution
(
    ReportExecutionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ReportExecution PRIMARY KEY,
    ReportName NVARCHAR(150) NOT NULL,
    ParametersJson NVARCHAR(MAX) NULL,
    ExecutedDate DATETIME2(0) NOT NULL,
    ExecutedByUserId BIGINT NOT NULL,
    OutputPath NVARCHAR(1000) NULL,
    ExecutionStatus NVARCHAR(50) NOT NULL,
    DurationMs INT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ReportExecution_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ReportExecution_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ReportExecution_ExecutedDate ON dbo.Trn_ReportExecution(ExecutedDate);
CREATE INDEX IX_Trn_ReportExecution_ExecutionStatus ON dbo.Trn_ReportExecution(ExecutionStatus);
GO

CREATE TABLE dbo.Trn_ClaimDailySnapshot
(
    ClaimDailySnapshotId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Trn_ClaimDailySnapshot PRIMARY KEY,
    SnapshotDate DATE NOT NULL,
    ClaimId BIGINT NOT NULL,
    CurrentStatusId BIGINT NOT NULL,
    ReserveAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL,
    OutstandingAmount DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Trn_ClaimDailySnapshot_IsActive DEFAULT (1),
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Trn_ClaimDailySnapshot_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2(0) NULL,
    ModifiedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_Trn_ClaimDailySnapshot_SnapshotDate ON dbo.Trn_ClaimDailySnapshot(SnapshotDate);
CREATE INDEX IX_Trn_ClaimDailySnapshot_ClaimId ON dbo.Trn_ClaimDailySnapshot(ClaimId);
GO
