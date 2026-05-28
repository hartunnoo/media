using Media.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Configurations;

public class MediaSearchIndexConfiguration : IEntityTypeConfiguration<MediaSearchIndex>
{
    public void Configure(EntityTypeBuilder<MediaSearchIndex> builder)
    {
        builder.HasKey(x => x.MediaId);
        builder.HasOne(x => x.Media).WithOne().HasForeignKey<MediaSearchIndex>(x => x.MediaId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.ExtractedText).HasColumnType("text");
        builder.Property(x => x.MetadataJson).HasColumnType("text");
    }
}
