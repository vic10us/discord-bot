using MassTransit;
using Microsoft.Extensions.Logging;
using v10.Events.Core.MessageBus.Contracts;

namespace v10.Events.Core.MessageBus.Consumers;

public class UpdateServerStatsCommandConsumer : IConsumer<UpdateServerStatsCommand>
{
    private readonly ILogger<UpdateServerStatsCommandConsumer> _logger;
    private readonly IBusControl _bus;

    public UpdateServerStatsCommandConsumer(ILogger<UpdateServerStatsCommandConsumer> logger, IBusControl bus)
    {
        _logger = logger;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<UpdateServerStatsCommand> context)
    {
        foreach (var guildId in context.Message.GuildIds)
        {
            _logger.LogInformation("Calling UpdateSingleServerStatsCommand for Guild with Id {guild}", guildId);
            await _bus.Publish(new UpdateSingleServerStatsCommand(guildId));
        }
    }
}
