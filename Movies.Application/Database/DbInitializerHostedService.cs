using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Movies.Application.Database;

public class DbInitializerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbInitializerHostedService> _logger;

    public DbInitializerHostedService(IServiceProvider serviceProvider, ILogger<DbInitializerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DbInitializerHostedService starting database initialization in background.");
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            await dbInitializer.InitializeAsync();
            _logger.LogInformation("Database initialization completed.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Database initialization canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed (background).");
        }
    }
}
