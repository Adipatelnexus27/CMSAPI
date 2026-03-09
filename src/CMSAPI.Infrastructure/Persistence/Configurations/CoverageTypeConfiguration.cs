using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class CoverageTypeConfiguration : IEntityTypeConfiguration<CoverageType>
{
    public void Configure(EntityTypeBuilder<CoverageType> builder)
    {
        builder.ToTable("Mst_CoverageType");
        builder.HasKey(x => x.CoverageTypeId);
        builder.Property(x => x.CoverageTypeId).ValueGeneratedOnAdd();

        builder.Property(x => x.CoverageCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CoverageName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CoverageDescription).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(x => x.CoverageCode).IsUnique();
    }
}

