using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class WorkflowStageMasterConfiguration : IEntityTypeConfiguration<WorkflowStageMaster>
{
    public void Configure(EntityTypeBuilder<WorkflowStageMaster> builder)
    {
        builder.ToTable("Mst_WorkflowStage");
        builder.HasKey(x => x.WorkflowStageId);
        builder.Property(x => x.WorkflowStageId).ValueGeneratedOnAdd();
        builder.Property(x => x.StageCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StageName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);
        builder.HasIndex(x => x.WorkflowDefinitionId);
        builder.HasIndex(x => x.StageCode).IsUnique();
    }
}

