using Kurvenanzeige.Core.Models;
using Microsoft.Extensions.Logging;

namespace Kurvenanzeige.Infrastructure.Services;

public class UiUpdateService
{
    private readonly ILogger<UiUpdateService> _logger;
    private readonly List<Func<List<PlcDataPoint>, Task>> _subscribers = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public UiUpdateService(ILogger<UiUpdateService> logger)
    {
        _logger = logger;
    }

    public async Task BroadcastUpdateAsync(List<PlcDataPoint> readings)
    {
        await _lock.WaitAsync();
        try
        {
            var tasks = _subscribers.Select(subscriber =>
            {
                try
                {
                    return subscriber(readings);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error notifying subscriber");
                    return Task.CompletedTask;
                }
            });

            await Task.WhenAll(tasks);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SubscribeAsync(Func<List<PlcDataPoint>, Task> callback)
    {
        await _lock.WaitAsync();
        try
        {
            _subscribers.Add(callback);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UnsubscribeAsync(Func<List<PlcDataPoint>, Task> callback)
    {
        await _lock.WaitAsync();
        try
        {
            _subscribers.Remove(callback);
        }
        finally
        {
            _lock.Release();
        }
    }
}
