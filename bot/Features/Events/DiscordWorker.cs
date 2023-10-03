using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using bot.Features.Events.Contracts;

namespace bot.Features.Events;

public class DiscordWorker : BackgroundService
{
    private readonly IBusControl _bus;

    public DiscordWorker(IBusControl bus)
    {
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new UpdateAllServerStatsCommand(), stoppingToken);

            await Task.Delay(10000, stoppingToken);
        }
    }
}
