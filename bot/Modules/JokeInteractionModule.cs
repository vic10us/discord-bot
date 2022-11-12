using System.Threading.Tasks;
using bot.Queries;
using Discord.Interactions;
using MediatR;

namespace bot.Modules;

public class JokeInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public JokeInteractionModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    [SlashCommand("joke", "Tell a joke")]
    public async Task TellJoke(JokeType jokeType)
    {
        var joke = jokeType switch
        {
            JokeType.Redneck => await _mediator.Send(new GetRedneckJokeResponse()),
            JokeType.Monday => await _mediator.Send(new GetMondayJokeResponse()),
            JokeType.Dad => (await _mediator.Send(new GetDadJokeResponse())).Joke,
            _ => (await _mediator.Send(new GetDadJokeResponse())).Joke,
        };
        await RespondAsync(joke);
    }
}
