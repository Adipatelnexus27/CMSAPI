using CMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMSAPI.Infrastructure.Persistence.Configurations;

public sealed class ClaimNoteConfiguration : IEntityTypeConfiguration<ClaimNote>
{
    public void Configure(EntityTypeBuilder<ClaimNote> builder)
    {
        builder.ToTable("Trn_ClaimNote");
        builder.HasKey(x => x.ClaimNoteId);
        builder.Property(x => x.ClaimNoteId).ValueGeneratedOnAdd();

        builder.Property(x => x.NoteCategory).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NoteText).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(x => x.ClaimId);
        builder.HasIndex(x => x.NotedDate);
    }
}
