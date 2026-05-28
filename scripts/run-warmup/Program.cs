using Media.Infrastructure;
using Media.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

// Run from published app directory
var appDir = @"C:\Users\hartu\apps\media";
Directory.SetCurrentDirectory(appDir);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
var host = builder.Build();

var warmup = host.Services.GetRequiredService<Media.Infrastructure.Services.ThumbnailWarmupService>();
Console.WriteLine("Starting warmup...");
await warmup.RunWarmupAsync(CancellationToken.None);
Console.WriteLine("Warmup complete.");
