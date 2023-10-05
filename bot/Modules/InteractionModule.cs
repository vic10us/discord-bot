using System.Threading.Tasks;
using bot.Queries;
using Discord.Interactions;
using MediatR;
using v10.Games.EightBall.Queries;

namespace bot.Modules;

public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public InteractionModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    [SlashCommand("ping", "Receive a ping message")]
    public async Task HandlePingCommand()
    {
        await RespondAsync("PONG!");
    }

    [SlashCommand("8ball", "Ask the magic 8 ball your question and see your future")]
    public async Task Handle8Ball(string question)
    {
        var x = await _mediator.Send(new GetRandom8BallResponse());
        await RespondAsync($"{question}: {x.Text}");
    }

}
