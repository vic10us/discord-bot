using System;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
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
        _database = server.Multiplexer.GetDatabase();
        _mediator = mediator;
        _logger = logger;
    }

    [SlashCommand("ping", "Receive a ping message")]
    public async Task HandlePingCommand()
    {
        if (!EnsureSingle()) { return; }
        await RespondAsync("PONG!");
        ReleaseLock();
    }

    [SlashCommand("8ball", "Ask the magic 8 ball your question and see your future")]
    public async Task Handle8Ball(string question)
    {
        if (!EnsureSingle()) { return; }
        try
        {
            var x = await _mediator.Send(new GetRandom8BallResponse());
            await RespondAsync($"{question}: {x.Text}");
        }
        finally
        {
            ReleaseLock();
        }
    }

}
