using System;
using System.Threading.Tasks;
using bot.Features.Caching;
using Discord.Commands;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Games.EightBall.Queries;

namespace bot.Modules;

public class InfoInteractionModule : CustomInteractionModule<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public InfoInteractionModule(
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<InfoInteractionModule> logger        
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        var database = server.Multiplexer.GetDatabase();
        _mediator = mediator;
        _logger = logger;
        _cacheContext = new CacheContext<SocketInteractionContext>(database, logger);
    }

    [SlashCommand("ping", "Receive a ping message")]
    public async Task HandlePingCommand()
    {
        await _cacheContext.WithLock(async () =>
        {
            await RespondAsync("PONG!");
        });
    }

    [SlashCommand("8ball", "Ask the magic 8 ball your question and see your future")]
    public async Task Handle8Ball(string question)
    {
        await _cacheContext.WithLock(async () =>
        {
            var x = await _mediator.Send(new GetRandom8BallResponse());
            await RespondAsync($"{question}: {x.Text}", ephemeral: true);
        });
    }

}
