using System;
using System.Threading.Tasks;
using bot.Features.Caching;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Services.DadJokes;
using v10.Services.MondayQuotes;
using v10.Services.RedneckJokes;

namespace bot.Modules;

[Group("joke")]
public class JokeGroupModule : CustomModule<SocketCommandContext>
{
    private readonly IDadJokeService _dadJokeService;
    private readonly IMondayQuotesService _mondayQuotesService;
    private readonly IRedneckJokeService _redneckJokeService;

    public JokeGroupModule(
        IDadJokeService dadJokeService,
        IMondayQuotesService mondayQuotesService,
        IRedneckJokeService redneckJokeService,
        IServiceProvider serviceProvider,
        ILogger<JokeGroupModule> logger
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        var database = server.Multiplexer.GetDatabase();
        _logger = logger;
        _dadJokeService = dadJokeService;
        _mondayQuotesService = mondayQuotesService;
        _redneckJokeService = redneckJokeService;
        _cacheContext = new CacheContext<SocketCommandContext>(database, logger);
    }

    [Command("dad")]
    [Alias("dj")]
    public async Task DadJoke()
    {
        await _cacheContext.WithLock(async () =>
        {
            var joke = await _dadJokeService.GetJokeAsync();
            await ReplyAsync(joke.Joke, messageReference: new MessageReference(Context.Message.Id));
        });
    }

    [Command("monday")]
    public async Task MondayQuote()
    {
        await _cacheContext.WithLock(async () =>
        {
            var joke = await _mondayQuotesService.GetQuote();
            await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
        });
    }

    [Command("redneck")]
    [Alias("rn")]
    public async Task RedneckJoke()
    {
        await _cacheContext.WithLock(async () =>
        {
            var joke = await _redneckJokeService.GetQuote();
            await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
        });
    }

    [Command]
    public async Task Help()
    {
        await _cacheContext.WithLock(async () =>
        {
            await ReplyAsync("use !joke [dad, redneck, monday]", messageReference: new MessageReference(Context.Message.Id));
        });
    }
}
