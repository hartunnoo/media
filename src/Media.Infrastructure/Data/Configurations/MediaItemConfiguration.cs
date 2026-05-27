using Media.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Configurations;

public class MediaItemConfiguration : IEntityTypeConfiguration<MediaItem>
{
    public void Configure(EntityTypeBuilder<MediaItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OriginalFileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FileHash).HasMaxLength(64);
        builder.Property(x => x.StoragePath).HasMaxLength(1000);
        builder.Property(x => x.OwnedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.OwnedByAppId).HasMaxLength(100);
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.Property(x => x.UpdatedBy).HasMaxLength(450);
        builder.Property(x => x.DeletedBy).HasMaxLength(450);
        builder.Property(x => x.RestoredBy).HasMaxLength(450);
        builder.HasOne(x => x.Folder).WithMany(f => f.MediaItems).HasForeignKey(x => x.FolderId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OwnedByUserId);
        builder.HasIndex(x => x.FileHash);
    }
}
