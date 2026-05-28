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

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();
app.MapHub<Media.Infrastructure.Hubs.MediaHub>("/hubs/media");
if (!string.IsNullOrEmpty(app.Configuration.GetConnectionString("MediaDb")))
{
    app.UseHangfireDashboard("/hangfire");
}

app.Run();
