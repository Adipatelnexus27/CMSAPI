using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMSAPI.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimParty> ClaimParties => Set<ClaimParty>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<ClaimNote> ClaimNotes => Set<ClaimNote>();
    public DbSet<ClaimTypeMaster> ClaimTypeMasters => Set<ClaimTypeMaster>();
    public DbSet<ClaimStatusMaster> ClaimStatusMasters => Set<ClaimStatusMaster>();
    public DbSet<InsuranceProduct> InsuranceProducts => Set<InsuranceProduct>();
    public DbSet<FraudRuleMaster> FraudRuleMasters => Set<FraudRuleMaster>();
    public DbSet<WorkflowStageMaster> WorkflowStageMasters => Set<WorkflowStageMaster>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyCoverage> PolicyCoverages => Set<PolicyCoverage>();
    public DbSet<CoverageType> CoverageTypes => Set<CoverageType>();
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<AuthRole> AuthRoles => Set<AuthRole>();
    public DbSet<AuthPermission> AuthPermissions => Set<AuthPermission>();
    public DbSet<AuthRolePermission> AuthRolePermissions => Set<AuthRolePermission>();
    public DbSet<AuthRefreshToken> AuthRefreshTokens => Set<AuthRefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
