using Media.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Configurations;

public class MediaAuditLogConfiguration : IEntityTypeConfiguration<MediaAuditLog>
{
    public void Configure(EntityTypeBuilder<MediaAuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.Changes).HasColumnType("text");
        builder.HasOne(x => x.Media).WithMany(m => m.AuditLogs).HasForeignKey(x => x.MediaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.MediaId);
        builder.HasIndex(x => x.Timestamp);
    }
}
