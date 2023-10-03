﻿using System.Threading.Tasks;
using bot.Features.Events.Contracts;
using Discord.WebSocket;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace bot.Features.Events.Consumers;

public class UpdateAllServerStatsCommandConsumer : IConsumer<UpdateAllServerStatsCommand>
{
    private readonly ILogger<UpdateAllServerStatsCommandConsumer> _logger;
    private readonly IBusControl _bus;
    private readonly DiscordSocketClient _discordSocketClient;
    //private readonly IMediator _mediator;
    //private readonly IDistributedCache _cache;

    public UpdateAllServerStatsCommandConsumer(
        ILogger<UpdateAllServerStatsCommandConsumer> logger
        ,IBusControl bus
        ,DiscordSocketClient discordSocketClient
        //,IMediator _mediator
        //,IDistributedCache cache
        )
    {
        _logger = logger;
        _bus = bus;
        _discordSocketClient = discordSocketClient;
        //_mediator = _mediator;
        //_cache = cache;
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