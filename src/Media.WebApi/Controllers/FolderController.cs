using Microsoft.AspNetCore.Mvc;

namespace Media.WebApi.Controllers;

[ApiController, Route("api/folders")]
public class FolderController : ControllerBase
{
    [HttpGet] public ActionResult List() => Ok(new[] { new { Id = Guid.NewGuid(), Name = "Root" } });
    [HttpPost] public ActionResult Create() => StatusCode(201);
}
