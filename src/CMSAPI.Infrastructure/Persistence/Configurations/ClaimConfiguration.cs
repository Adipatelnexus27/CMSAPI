using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClaimNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.ClaimNumber)
            .IsUnique();

        builder.Property(x => x.PolicyNumber)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.ClaimantName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.ClaimedAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ReservedAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Description)
            .HasMaxLength(1000);
    }
}

