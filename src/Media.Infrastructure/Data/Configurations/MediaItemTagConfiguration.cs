using Media.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Configurations;

public class MediaItemTagConfiguration : IEntityTypeConfiguration<MediaItemTag>
{
    public void Configure(EntityTypeBuilder<MediaItemTag> builder)
    {
        builder.HasKey(x => new { x.MediaItemId, x.MediaTagId });
        builder.HasOne(x => x.MediaItem).WithMany(m => m.MediaItemTags).HasForeignKey(x => x.MediaItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.MediaTag).WithMany(t => t.MediaItemTags).HasForeignKey(x => x.MediaTagId).OnDelete(DeleteBehavior.Cascade);
    }
}
