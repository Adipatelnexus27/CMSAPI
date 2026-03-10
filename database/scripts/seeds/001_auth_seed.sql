DECLARE @AdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @ClaimManagerRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @InvestigatorRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @AdjusterRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @FinanceRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @FraudAnalystRoleId UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.Roles(RoleId, Name)
VALUES
(@AdminRoleId, 'Admin'),
(@ClaimManagerRoleId, 'Claim Manager'),
(@InvestigatorRoleId, 'Investigator'),
(@AdjusterRoleId, 'Adjuster'),
(@FinanceRoleId, 'Finance'),
(@FraudAnalystRoleId, 'Fraud Analyst');

DECLARE @UsersManage UNIQUEIDENTIFIER = NEWID();
DECLARE @ClaimsRead UNIQUEIDENTIFIER = NEWID();
DECLARE @ClaimsAssign UNIQUEIDENTIFIER = NEWID();
DECLARE @ClaimsInvestigate UNIQUEIDENTIFIER = NEWID();
DECLARE @ClaimsAdjust UNIQUEIDENTIFIER = NEWID();
DECLARE @PaymentsProcess UNIQUEIDENTIFIER = NEWID();
DECLARE @FraudReview UNIQUEIDENTIFIER = NEWID();
DECLARE @ReportsView UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.Permissions(PermissionId, Name)
VALUES
(@UsersManage, 'Users.Manage'),
(@ClaimsRead, 'Claims.Read'),
(@ClaimsAssign, 'Claims.Assign'),
(@ClaimsInvestigate, 'Claims.Investigate'),
(@ClaimsAdjust, 'Claims.Adjust'),
(@PaymentsProcess, 'Payments.Process'),
(@FraudReview, 'Fraud.Review'),
(@ReportsView, 'Reports.View');

INSERT INTO dbo.RolePermissions(RolePermissionId, RoleId, PermissionId)
VALUES
(NEWID(), @AdminRoleId, @UsersManage),
(NEWID(), @AdminRoleId, @ClaimsRead),
(NEWID(), @AdminRoleId, @ClaimsAssign),
(NEWID(), @AdminRoleId, @ClaimsInvestigate),
(NEWID(), @AdminRoleId, @ClaimsAdjust),
(NEWID(), @AdminRoleId, @PaymentsProcess),
(NEWID(), @AdminRoleId, @FraudReview),
(NEWID(), @AdminRoleId, @ReportsView),
(NEWID(), @ClaimManagerRoleId, @ClaimsRead),
(NEWID(), @ClaimManagerRoleId, @ClaimsAssign),
(NEWID(), @ClaimManagerRoleId, @ReportsView),
(NEWID(), @InvestigatorRoleId, @ClaimsRead),
(NEWID(), @InvestigatorRoleId, @ClaimsInvestigate),
(NEWID(), @AdjusterRoleId, @ClaimsRead),
(NEWID(), @AdjusterRoleId, @ClaimsAdjust),
(NEWID(), @FinanceRoleId, @ClaimsRead),
(NEWID(), @FinanceRoleId, @PaymentsProcess),
(NEWID(), @FraudAnalystRoleId, @ClaimsRead),
(NEWID(), @FraudAnalystRoleId, @FraudReview);

