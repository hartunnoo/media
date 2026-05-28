using Media.Application.Commands.RenameMedia;
using Media.Application.Commands.RestoreMedia;
using Media.Application.Commands.SoftDeleteMedia;
using Media.Application.Commands.SwapMedia;
using Media.Application.Commands.UploadMedia;
using Media.Application.DTOs;
using Media.Application.Queries.GetMediaById;
using Media.Application.Queries.ListMedia;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Media.WebApi.Controllers;

[ApiController, Route("api/media")]
public class MediaController(IMediator mediator) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadResultDto>> Upload(
        IFormFile file, [FromForm] Guid? folderId, [FromForm] string ownedByUserId = "system", [FromForm] string? ownedByAppId = null)
    {
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadMediaCommand(stream, file.FileName, file.ContentType, folderId, ownedByUserId, ownedByAppId));
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MediaItemDto>>> List([FromQuery] string? search, [FromQuery] Guid? folderId, [FromQuery] string? ownedByUserId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        => Ok(await mediator.Send(new ListMediaQuery(search, folderId, ownedByUserId, skip, take)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MediaItemDto>> Get(Guid id)
    {
        var item = await mediator.Send(new GetMediaByIdQuery(id));
        return item is not null ? Ok(item) : NotFound();
    }

    [HttpGet("{id:guid}/download")]
    public async Task<ActionResult> Download(Guid id, [FromServices] Media.Application.Interfaces.IFileStorageService fileStorage)
    {
        var item = await mediator.Send(new GetMediaByIdQuery(id));
        if (item is null) return NotFound();

        // Files are stored as "original.{ext}" inside the mediaId folder
        var ext = Path.GetExtension(item.OriginalFileName);
        if (string.IsNullOrEmpty(ext)) ext = ".bin";
        var stream = await fileStorage.GetStreamAsync(id, $"original{ext}");
        if (stream is null) return NotFound();

        return File(stream, item.ContentType, item.OriginalFileName);
    }

    [HttpGet("{id:guid}/thumbnail")]
    public async Task<ActionResult> Thumbnail(Guid id,
        [FromServices] Media.Application.Interfaces.IThumbnailService thumbnailService,
        [FromServices] Media.Application.Interfaces.IFileStorageService fileStorage,
        [FromQuery] string size = "small")
    {
        // Try cached thumbnail first
        var thumb = await thumbnailService.GetThumbnailAsync(id, size);
        if (thumb is not null) return File(thumb, "image/webp");

        // Fall back to full image (generate thumbnail async for next time)
        var item = await mediator.Send(new GetMediaByIdQuery(id));
        if (item is null) return NotFound();
        var ext = Path.GetExtension(item.OriginalFileName);
        if (string.IsNullOrEmpty(ext)) ext = ".bin";
        var stream = await fileStorage.GetStreamAsync(id, $"original{ext}");
        if (stream is null) return NotFound();

        // Generate thumbnail in background for future requests
        _ = Task.Run(async () => {
            try { stream.Position = 0; var thumbnails = await thumbnailService.GenerateAsync(stream); await thumbnailService.SaveThumbnailsAsync(id, thumbnails); }
            catch { /* best effort */ }
        });

        stream.Position = 0;
        return File(stream, item.ContentType, item.OriginalFileName);
    }

    [HttpPut("{id:guid}/rename")]
    public async Task<ActionResult> Rename(Guid id, [FromBody] RenameRequest req)
    {
        await mediator.Send(new RenameMediaCommand(id, req.NewFileName));
        return NoContent();
    }

    [HttpPut("{id:guid}/swap")]
    public async Task<ActionResult> Swap(Guid id, [FromForm] string? comment)
    {
        var file = HttpContext.Request.Form.Files[0];
        await using var stream = file.OpenReadStream();
        await mediator.Send(new SwapMediaCommand(id, stream, file.ContentType, comment));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> SoftDelete(Guid id, [FromQuery] string deletedBy = "system")
    {
        await mediator.Send(new SoftDeleteMediaCommand(id, deletedBy));
        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult> Restore(Guid id, [FromQuery] string restoredBy = "system")
    {
        await mediator.Send(new RestoreMediaCommand(id, restoredBy));
        return NoContent();
    }
}

public record RenameRequest(string NewFileName);
