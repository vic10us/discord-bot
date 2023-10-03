using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using bot.Features.Events.Contracts;
using System.Collections.Generic;

namespace bot.Features.Events;

public class DiscordWorker : BackgroundService
{
    private readonly ILogger<DiscordWorker> _logger;
    private readonly IBusControl _bus;
    private readonly IEnumerable<IConsumer> _consumers;

    public DiscordWorker(ILogger<DiscordWorker> logger, IBusControl bus, IEnumerable<IConsumer> consumers)
    {
        _logger = logger;
        _bus = bus;
        _consumers = consumers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // await _bus.StartAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new UpdateAllServerStatsCommand(), stoppingToken);

            await Task.Delay(10000, stoppingToken);
        }

        // await _bus.StopAsync(stoppingToken);
    }
}
