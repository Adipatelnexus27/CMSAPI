using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class InsuranceProductConfiguration : IEntityTypeConfiguration<InsuranceProduct>
{
    public void Configure(EntityTypeBuilder<InsuranceProduct> builder)
    {
        builder.ToTable("Mst_PolicyType");
        builder.HasKey(x => x.PolicyTypeId);
        builder.Property(x => x.PolicyTypeId).ValueGeneratedOnAdd();
        builder.Property(x => x.PolicyTypeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PolicyTypeName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PolicyTypeDescription).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);
        builder.HasIndex(x => x.PolicyTypeCode).IsUnique();
    }
}

