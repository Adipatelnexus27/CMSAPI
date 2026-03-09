using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class FraudRuleMasterConfiguration : IEntityTypeConfiguration<FraudRuleMaster>
{
    public void Configure(EntityTypeBuilder<FraudRuleMaster> builder)
    {
        builder.ToTable("Mst_FraudRule");
        builder.HasKey(x => x.FraudRuleId);
        builder.Property(x => x.FraudRuleId).ValueGeneratedOnAdd();
        builder.Property(x => x.RuleCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RuleName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.RuleDefinition).HasMaxLength(2000);
        builder.Property(x => x.RuleWeight).HasColumnType("decimal(5,2)");
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);
        builder.HasIndex(x => x.RuleCode).IsUnique();
    }
}

