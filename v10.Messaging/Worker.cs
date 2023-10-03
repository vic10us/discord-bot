using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using v10.Messaging.Contracts;

namespace v10.Messaging;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBusControl _bus;

    public Worker(ILogger<Worker> logger, IBusControl bus)
    {
        _logger = logger;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _bus.StartAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new HelloMessage { Message = "Hello, World!" }, stoppingToken);

            await Task.Delay(60000, stoppingToken);
        }

        await _bus.StopAsync(stoppingToken);
    }
}
