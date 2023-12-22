using System;
using System.Threading.Tasks;
using bot.Features.Caching;
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
        var database = server.Multiplexer.GetDatabase();
        _logger = logger;
        _mediator = mediator;
        _cacheContext = new CacheContext<SocketCommandContext>(database, logger);
    }

    [Command("dadjoke")]
    [Alias("dj")]
    public async Task DadJoke()
    {
        await _cacheContext.WithLock(async () =>
        {
            var joke = await _mediator.Send(new GetDadJokeResponse());
            await ReplyAsync(joke.Joke, messageReference: new MessageReference(Context.Message.Id));
        });
    }

    [Command("monday")]
    public async Task MondayQuote()
    {
        await _cacheContext.WithLock(async () =>
        {
            var joke = await _mediator.Send(new GetMondayJokeResponse());
            await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
        });
    }

    [Command("redneckjoke")]
    [Alias("redneck", "rn")]
    public async Task RedneckJoke()
    {
        await _cacheContext.WithLock(async () =>
        {
            var joke = await _mediator.Send(new GetRedneckJokeResponse());
            await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
        });
    }
}
