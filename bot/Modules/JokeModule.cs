using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using v10.Services.DadJokes.Queries;
using v10.Services.MondayQuotes.Queries;
using v10.Services.RedneckJokes.Queries;

namespace bot.Modules;

public class JokeModule : CustomModule<SocketCommandContext>
{
    private readonly IMediator _mediator;

    public JokeModule(IMediator mediator)
    {
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
