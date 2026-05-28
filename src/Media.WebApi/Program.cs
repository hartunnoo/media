using Media.Infrastructure;
using Media.Application;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.OperationFilter<Media.WebApi.SwaggerFileOperationFilter>());
builder.Services.AddSignalR();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
