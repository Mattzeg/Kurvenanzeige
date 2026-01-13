using Kurvenanzeige.Core.Configuration;
using Kurvenanzeige.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kurvenanzeige.Infrastructure.Services;

public class DataPollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPlcService _plcService;
    private readonly DataPollingConfig _config;
    private readonly ILogger<DataPollingService> _logger;
    private readonly UiUpdateService _uiUpdateService;

    public DataPollingService(
        IServiceProvider serviceProvider,
        IPlcService plcService,
        IOptions<DataPollingConfig> config,
        ILogger<DataPollingService> logger,
        UiUpdateService uiUpdateService)
    {
        _serviceProvider = serviceProvider;
        _plcService = plcService;
        _config = config.Value;
        _logger = logger;
        _uiUpdateService = uiUpdateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataPollingService starting...");

        await Task.Delay(2000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollDataAsync(stoppingToken);
                await Task.Delay(_config.PollingIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("DataPollingService stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in polling cycle");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task PollDataAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDataRepository>();

        if (!_plcService.IsConnected)
        {
            _logger.LogWarning("PLC not connected, attempting to connect...");
            var connected = await _plcService.ConnectAsync(ct);

            if (!connected)
            {
                _logger.LogWarning("Failed to connect to PLC, will retry in next cycle");
                return;
            }
        }

        var dataPoints = await repository.GetEnabledDataPointsAsync();

        if (!dataPoints.Any())
        {
            _logger.LogWarning("No enabled data points configured");
            return;
        }

        _logger.LogDebug("Reading {Count} data points from PLC", dataPoints.Count);

        var readings = await _plcService.ReadMultipleAsync(dataPoints, ct);

        if (readings.Any())
        {
            await repository.BulkInsertReadingsAsync(readings, ct);

            await _uiUpdateService.BroadcastUpdateAsync(readings);

            var goodReadings = readings.Count(r => r.Quality == Core.Models.Quality.Good);
            _logger.LogDebug("Successfully read {Good}/{Total} data points", goodReadings, readings.Count);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DataPollingService is stopping");

        await _plcService.DisconnectAsync();

        await base.StopAsync(cancellationToken);
    }
}
