using Discord.WebSocket;
using MassTransit;
using Microsoft.Extensions.Logging;
using v10.Events.Core.MessageBus.Contracts;

namespace v10.Events.Core.MessageBus.Consumers;

public class UpdateAllServerStatsCommandConsumer : IConsumer<UpdateAllServerStatsCommand>
{
    private readonly ILogger<UpdateAllServerStatsCommandConsumer> _logger;
    private readonly IBusControl _bus;
    private readonly DiscordSocketClient _discordSocketClient;

    public UpdateAllServerStatsCommandConsumer(
        ILogger<UpdateAllServerStatsCommandConsumer> logger
        , IBusControl bus
        , DiscordSocketClient discordSocketClient
        )
    {
        _logger = logger;
        _bus = bus;
        _discordSocketClient = discordSocketClient;
    }

    public async Task Consume(ConsumeContext<UpdateAllServerStatsCommand> context)
    {
        var guilds = _discordSocketClient.Guilds;
        foreach (var guild in guilds)
        {
            _logger.LogInformation("Calling UpdateSingleServerStatsCommand for Guild with Id {Id}", guild.Id);
            await _bus.Publish(new UpdateSingleServerStatsCommand(guild.Id));
        }
    }
}
