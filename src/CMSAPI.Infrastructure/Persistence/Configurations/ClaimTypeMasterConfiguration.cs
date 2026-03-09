using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class ClaimTypeMasterConfiguration : IEntityTypeConfiguration<ClaimTypeMaster>
{
    public void Configure(EntityTypeBuilder<ClaimTypeMaster> builder)
    {
        builder.ToTable("Mst_ClaimType");
        builder.HasKey(x => x.ClaimTypeId);
        builder.Property(x => x.ClaimTypeId).ValueGeneratedOnAdd();
        builder.Property(x => x.ClaimTypeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ClaimTypeName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ClaimTypeDescription).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);
        builder.HasIndex(x => x.ClaimTypeCode).IsUnique();
    }
}

