namespace Media.Application.Interfaces;

public interface IMediaProcessingService
{
    Task ProcessAsync(Guid mediaId, CancellationToken ct = default);
}
