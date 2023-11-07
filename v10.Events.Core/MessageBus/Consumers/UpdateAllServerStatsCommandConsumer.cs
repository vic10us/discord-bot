using Discord.WebSocket;
using MassTransit;
using Microsoft.Extensions.Logging;
using v10.Data.MongoDB;
using v10.Events.Core.MessageBus.Contracts;

namespace v10.Events.Core.MessageBus.Consumers;

public class UpdateAllServerStatsCommandConsumer : IConsumer<UpdateAllServerStatsCommand>
{
    private readonly ILogger<UpdateAllServerStatsCommandConsumer> _logger;
    private readonly IBusControl _bus;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IBotDataService _botDataService;

    public UpdateAllServerStatsCommandConsumer(
        ILogger<UpdateAllServerStatsCommandConsumer> logger
        , IBusControl bus
        , DiscordSocketClient discordSocketClient
        ,IBotDataService botDataService)
    {
        _logger = logger;
        _bus = bus;
        _discordSocketClient = discordSocketClient;
        _botDataService = botDataService;
    }

    public async Task Consume(ConsumeContext<UpdateAllServerStatsCommand> context)
    {
        var guilds = _discordSocketClient.Guilds;
        var configuredGuilds = await _botDataService.GetGuildsAsync();
        var guildIds = guilds.Select(g => g.Id).ToList();
        guildIds.AddRange(configuredGuilds.Select(g => ulong.Parse(g.guildId)));
        guildIds = guildIds.Distinct().ToList();
        foreach (var guild in guildIds)
        {
            _logger.LogInformation("Calling UpdateSingleServerStatsCommand for Guild with Id {Id}", guild);
            await _bus.Publish(new UpdateSingleServerStatsCommand(guild));
        }
    }
}
