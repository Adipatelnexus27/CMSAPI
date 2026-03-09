using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class ClaimStatusMasterConfiguration : IEntityTypeConfiguration<ClaimStatusMaster>
{
    public void Configure(EntityTypeBuilder<ClaimStatusMaster> builder)
    {
        builder.ToTable("Mst_ClaimStatus");
        builder.HasKey(x => x.ClaimStatusId);
        builder.Property(x => x.ClaimStatusId).ValueGeneratedOnAdd();
        builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StatusName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);
        builder.HasIndex(x => x.StatusCode).IsUnique();
        builder.HasIndex(x => x.SequenceNo);
    }
}

