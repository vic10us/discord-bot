using MassTransit;
using Microsoft.Extensions.Logging;
using v10.Messaging.Contracts;

namespace v10.Messaging.Consumers;

public class HelloMessageConsumer : 
    IConsumer<HelloMessage>
{
    private readonly ILogger<HelloMessageConsumer> _logger;

    public HelloMessageConsumer(ILogger<HelloMessageConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<HelloMessage> context)
    {
        _logger.LogInformation("Consumed: {Message}!", context.Message.Message);

        return Task.CompletedTask;
    }
}
