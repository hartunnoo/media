namespace Media.Domain.Entities;

public class MediaSearchIndex
{
    public Guid MediaId { get; set; }
    public MediaItem Media { get; set; } = null!;
    public string? ExtractedText { get; set; }
    public string? MetadataJson { get; set; }
}
