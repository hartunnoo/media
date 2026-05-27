namespace Media.Application.Interfaces;

public interface IThumbnailService
{
    Task<Dictionary<string, byte[]>> GenerateAsync(Stream imageStream, CancellationToken ct = default);
    Task<byte[]?> GetThumbnailAsync(Guid mediaId, string size, CancellationToken ct = default);
}
