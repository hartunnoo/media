using Media.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Configurations;

public class MediaVersionConfiguration : IEntityTypeConfiguration<MediaVersion>
{
    public void Configure(EntityTypeBuilder<MediaVersion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(1000);
        builder.Property(x => x.CreatedBy).HasMaxLength(450);
        builder.HasOne(x => x.Media).WithMany(m => m.Versions).HasForeignKey(x => x.MediaId).OnDelete(DeleteBehavior.Cascade);
    }
}
