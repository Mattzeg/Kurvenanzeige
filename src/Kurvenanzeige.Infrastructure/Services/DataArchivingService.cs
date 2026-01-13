using Kurvenanzeige.Core.Configuration;
using Kurvenanzeige.Core.Interfaces;
using Kurvenanzeige.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurvenanzeige.Infrastructure.Services;

public class DataArchivingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DataRetentionConfig _config;
    private readonly ILogger<DataArchivingService> _logger;

    public DataArchivingService(
        IServiceProvider serviceProvider,
        IOptions<DataRetentionConfig> config,
        ILogger<DataArchivingService> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataArchivingService starting...");

        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromHours(_config.CleanupIntervalHours), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("DataArchivingService stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in archiving cycle");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task PerformCleanupAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _logger.LogInformation("Starting data cleanup...");

        var cutoffDate = DateTime.Now.AddDays(-_config.RawDataRetentionDays);
        await repository.DeleteOldReadingsAsync(cutoffDate);

        if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday && DateTime.Now.Hour >= 2 && DateTime.Now.Hour < 3)
        {
            _logger.LogInformation("Performing weekly VACUUM...");
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync("VACUUM;", ct);
                _logger.LogInformation("VACUUM completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing VACUUM");
            }
        }

        _logger.LogInformation("Data cleanup completed");
    }
}
