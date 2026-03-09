using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Trn_Claim");
        builder.HasKey(x => x.ClaimId);
        builder.Property(x => x.ClaimId).ValueGeneratedOnAdd();

        builder.Property(x => x.ClaimNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.ClaimNumber)
            .IsUnique();

        builder.Property(x => x.IncidentDescription)
            .HasMaxLength(2000);

        builder.Property(x => x.LocationOfLoss)
            .HasMaxLength(500);

        builder.Property(x => x.EstimatedLossAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ApprovedLossAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.FraudScore)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(100);

        builder.HasIndex(x => x.PolicyId);
        builder.HasIndex(x => x.ClaimTypeId);
        builder.HasIndex(x => x.CurrentStatusId);
        builder.HasIndex(x => x.ReportedDate);
        builder.HasIndex(x => x.LossDate);
    }
}
