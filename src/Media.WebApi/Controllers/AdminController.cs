using Microsoft.AspNetCore.Mvc;

namespace Media.WebApi.Controllers;

[ApiController, Route("api/admin")]
public class AdminController : ControllerBase
{
    [HttpGet("quotas")] public ActionResult Quotas() => Ok(new { used = "0 GB", limit = "5 GB" });
    [HttpGet("health")] public ActionResult Health() => Ok(new { status = "healthy", db = "connected", storage = "ok" });
}
