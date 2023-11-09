using System;
using System.Linq;
using System.Threading.Tasks;
using bot.Features.Caching;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using StackExchange.Redis;
using v10.Events.Core.Enums;
using v10.Services.DadJokes.Queries;
using v10.Services.MondayQuotes.Queries;
using v10.Services.RedneckJokes.Queries;
using v10.Services.StrangeLaws.Queries;

namespace bot.Modules;

public class JokeInteractionModule : CustomInteractionModule<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public JokeInteractionModule(
        IMediator mediator, 
        ILogger<JokeInteractionModule> logger,
        IServiceProvider serviceProvider
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        var database = server.Multiplexer.GetDatabase();
        _mediator = mediator;
        _logger = logger;
        _cacheContext = new CacheContext<SocketInteractionContext>(database, logger);
    }

    private static JokeType GetRandomJokeType()
    {
        var jokeTypes = Enum.GetValues<JokeType>().Where(jt => jt != JokeType.Random).ToArray();
        var random = new Random();
        var position = random.Next(0, jokeTypes.Length);
        return jokeTypes[position];
    }

    [SlashCommand("joke", "Tell a joke")]
    public async Task TellJoke(JokeType jokeType)
    {
        await _cacheContext.WithLock(async () =>
        {
            await TellJokeAsync(jokeType);
        });
    }

    private async Task TellJokeAsync(JokeType jokeType)
    {
        await DeferAsync();
        if (jokeType == JokeType.Random) jokeType = GetRandomJokeType();
        var joke = jokeType switch
        {
            JokeType.Redneck => await _mediator.Send(new GetRedneckJokeResponse()),
            JokeType.Monday => await _mediator.Send(new GetMondayJokeResponse()),
            JokeType.Dad => (await _mediator.Send(new GetDadJokeResponse())).Joke,
            JokeType.StrangeLaw => (await _mediator.Send(new GetStrangeLawResponse())),
            _ => (await _mediator.Send(new GetDadJokeResponse())).Joke,
        };
        var builder = new ComponentBuilder()
        .WithButton($"Another {jokeType} Joke!", $"joke:{jokeType}", ButtonStyle.Success);
        await FollowupAsync(joke, components: builder.Build());
    }

    [ComponentInteraction("joke:*")]
    public async Task JokeButtonInteraction(string type)
    {
        await _cacheContext.WithLock(async () =>
        {
            _logger.LogInformation("User pressed the {type} joke button", type);
            var jokeType = Enum.Parse<JokeType>(type, true);
            await TellJokeAsync(jokeType);
        });
    }
}
