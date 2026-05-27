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
    public async Task<ActionResult<UploadResultDto>> Upload([FromForm] IFormFile file, [FromForm] Guid? folderId, [FromForm] string ownedByUserId = "system", [FromForm] string? ownedByAppId = null)
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
    public async Task<ActionResult> Download(Guid id)
    {
        var item = await mediator.Send(new GetMediaByIdQuery(id));
        if (item is null) return NotFound();
        return Ok(new { downloadUrl = $"/api/media/{id}/stream" });
    }

    [HttpPut("{id:guid}/rename")]
    public async Task<ActionResult> Rename(Guid id, [FromBody] RenameRequest req)
    {
        await mediator.Send(new RenameMediaCommand(id, req.NewFileName));
        return NoContent();
    }

    [HttpPut("{id:guid}/swap")]
    public async Task<ActionResult> Swap(Guid id, [FromForm] IFormFile file, [FromForm] string? comment)
    {
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
