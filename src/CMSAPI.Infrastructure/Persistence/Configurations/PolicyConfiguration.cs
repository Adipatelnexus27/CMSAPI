using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Mst_Policy");
        builder.HasKey(x => x.PolicyId);
        builder.Property(x => x.PolicyId).ValueGeneratedOnAdd();

        builder.Property(x => x.PolicyNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.InsuredName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PolicyStatus).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SumInsured).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(x => x.PolicyNumber).IsUnique();
        builder.HasIndex(x => x.PolicyTypeId);
        builder.HasIndex(x => x.CurrencyId);
        builder.HasIndex(x => x.PolicyEndDate);
    }
}

