using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class AuthRolePermissionConfiguration : IEntityTypeConfiguration<AuthRolePermission>
{
    public void Configure(EntityTypeBuilder<AuthRolePermission> builder)
    {
        builder.ToTable("Mst_RolePermission");
        builder.HasKey(x => x.RolePermissionId);
        builder.Property(x => x.RolePermissionId).ValueGeneratedOnAdd();

        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => x.PermissionId);
        builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
    }
}

