using System;
using System.Linq;
using System.Threading.Tasks;
using bot.Modules.Enums;
using bot.Queries;
using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;

namespace bot.Modules;

public class JokeInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;
    private readonly ILogger<JokeInteractionModule> _logger;

    public JokeInteractionModule(IMediator mediator, ILogger<JokeInteractionModule> logger)
    {
        _mediator = mediator;
        _logger = logger;
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

        await RespondAsync(joke, components: builder.Build());
    }

    [ComponentInteraction("joke:*")]
    public async Task JokeButtonInteraction(string type)
    {
        _logger.LogInformation("User pressed the {type} joke button", type);
        var jokeType = Enum.Parse<JokeType>(type, true);
        await TellJoke(jokeType);
    }
}
