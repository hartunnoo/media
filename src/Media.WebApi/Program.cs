using Media.Infrastructure;
using Media.Application;
using Hangfire;

// Detect published app directory for Windows Service
var mediaAppDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
var mediaWebRoot = Path.Combine(mediaAppDir, "wwwroot");
if (Directory.Exists(mediaWebRoot))
{
    Directory.SetCurrentDirectory(mediaAppDir);
    args = [..args, "--contentRoot", mediaAppDir, "--webRoot", mediaWebRoot];
}

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.OperationFilter<Media.WebApi.SwaggerFileOperationFilter>());
builder.Services.AddSignalR();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// Require API key for mutation endpoints — GET/download/thumbnail remain public
app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
    {
        var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
        var expected = app.Configuration.GetValue<string>("ApiKey") ?? "hmo-media-api-key-2026";
        if (key != expected)
        {
            ctx.Response.StatusCode = 401;
            return;
        }
    }
    await next();
});

app.MapControllers();
app.MapHub<Media.Infrastructure.Hubs.MediaHub>("/hubs/media");
if (!string.IsNullOrEmpty(app.Configuration.GetConnectionString("MediaDb")))
{
    app.UseHangfireDashboard("/hangfire");
}

// ── Status page restart endpoints (fallback if Pesambah is down) ───
app.MapPost("/api/admin/restart-service", async (HttpContext ctx) =>
{
    var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
    if (key != (app.Configuration["InternalApi:Key"] ?? "hmo-sso-internal-sync-key-2026-secure-v2"))
        return Results.Unauthorized();
    var svc = ctx.Request.Query["svc"].ToString();
    var allowed = new[] { "KoleksiPesambah","KoleksiAlbumKDYMM","HmoSso","SSCU","HmoMediaApi","ProjectTracker" };
    if (!allowed.Contains(svc, StringComparer.OrdinalIgnoreCase)) return Results.BadRequest("Unknown");
    try
    {
        System.Diagnostics.Process.Start("cmd.exe", $"/c sc.exe stop {svc} & sc.exe start {svc}");
        return Results.Ok(new { service = svc });
    }
    catch (Exception ex) { return Results.Problem(ex.Message); }
}).RequireHost("*");

app.MapPost("/api/admin/restart-tunnel", async (HttpContext ctx) =>
{
    var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
    if (key != (app.Configuration["InternalApi:Key"] ?? "hmo-sso-internal-sync-key-2026-secure-v2"))
        return Results.Unauthorized();
    System.Diagnostics.Process.Start("powershell.exe", "-Command \"Get-Process cloudflared -EA 0 | Stop-Process -Force; Start-Process 'C:\\Program Files (x86)\\cloudflared\\cloudflared.exe' -ArgumentList '--config C:\\Users\\hartu\\.cloudflared\\config.yml tunnel run' -NoNewWindow\"");
    return Results.Ok(new { status = "ok" });
}).RequireHost("*");

app.Run();

