using Media.Application.Interfaces;
using Media.Domain.Interfaces;
using Media.Infrastructure.Data;
using Media.Infrastructure.Repositories;
using Media.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Media.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MediaDb")
            ?? throw new InvalidOperationException("Connection string 'MediaDb' not found.");

        services.AddDbContext<MediaDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MediaDbContext>());
        services.AddScoped<IMediaRepository, MediaRepository>();

        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddSingleton<IThumbnailService, ThumbnailService>();
        services.AddScoped<IMediaProcessingService, MediaProcessingService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
        services.AddHangfireServer();

        return services;
    }
}
