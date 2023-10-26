using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using v10.Events.Core.MessageBus.Contracts;

namespace v10.Events.Core;

public class DiscordWorker : BackgroundService
{
    private readonly IBusControl _bus;
    private readonly DiscordWorkerOptions _options;

    public DiscordWorker(IBusControl bus, IOptions<DiscordWorkerOptions> options)
    {
        _bus = bus;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new UpdateAllServerStatsCommand(), stoppingToken);

            await Task.Delay(_options.IntervalInMiliseconds, stoppingToken);
        }
    }
}

public class DiscordWorkerOptions
{
    public int IntervalInMiliseconds { get; set; } = 10000;
}
