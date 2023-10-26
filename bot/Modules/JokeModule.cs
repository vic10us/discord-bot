using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Services.DadJokes.Queries;
using v10.Services.MondayQuotes.Queries;
using v10.Services.RedneckJokes.Queries;

namespace bot.Modules;

public class JokeModule : CustomModule<SocketCommandContext>
{
    private readonly IMediator _mediator;

    public JokeModule(
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<JokeModule> logger
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        _database = server.Multiplexer.GetDatabase();
        _logger = logger;
        _mediator = mediator;
    }

    [Command("dadjoke")]
    [Alias("dj")]
    public async Task DadJoke()
    {
        var joke = await _mediator.Send(new GetDadJokeResponse());
        await ReplyAsync(joke.Joke, messageReference: new MessageReference(Context.Message.Id));
    }

    [Command("monday")]
    public async Task MondayQuote()
    {
        var joke = await _mediator.Send(new GetMondayJokeResponse());
        await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
    }

    [Command("redneckjoke")]
    [Alias("redneck", "rn")]
    public async Task RedneckJoke()
    {
        var joke = await _mediator.Send(new GetRedneckJokeResponse());
        await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
    }
}
