using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class ClaimAssignmentConfiguration : IEntityTypeConfiguration<ClaimAssignment>
{
    public void Configure(EntityTypeBuilder<ClaimAssignment> builder)
    {
        builder.ToTable("Trn_ClaimAssignment");
        builder.HasKey(x => x.ClaimAssignmentId);
        builder.Property(x => x.ClaimAssignmentId).ValueGeneratedOnAdd();

        builder.Property(x => x.AssignmentReason).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(x => x.ClaimId);
        builder.HasIndex(x => x.AssignedToUserId);
        builder.HasIndex(x => x.IsCurrent);
    }
}
