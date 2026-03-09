using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class AuthPermissionConfiguration : IEntityTypeConfiguration<AuthPermission>
{
    public void Configure(EntityTypeBuilder<AuthPermission> builder)
    {
        builder.ToTable("Mst_Permission");
        builder.HasKey(x => x.PermissionId);
        builder.Property(x => x.PermissionId).ValueGeneratedOnAdd();

        builder.Property(x => x.PermissionCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PermissionName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PermissionDescription).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(x => x.PermissionCode).IsUnique();
    }
}

