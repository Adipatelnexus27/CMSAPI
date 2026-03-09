using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class PolicyCoverageConfiguration : IEntityTypeConfiguration<PolicyCoverage>
{
    public void Configure(EntityTypeBuilder<PolicyCoverage> builder)
    {
        builder.ToTable("Mst_PolicyCoverage");
        builder.HasKey(x => x.PolicyCoverageId);
        builder.Property(x => x.PolicyCoverageId).ValueGeneratedOnAdd();

        builder.Property(x => x.CoverageLimit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DeductibleAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(x => x.PolicyId);
        builder.HasIndex(x => x.CoverageTypeId);
    }
}

