using System.Threading.Tasks;
using bot.Features.DadJokes;
using bot.Features.MondayQuotes;
using bot.Features.RedneckJokes;
using Discord;
using Discord.Commands;

namespace bot.Modules;

public class JokeModule : CustomModule<SocketCommandContext>
{
    public DadJokeService DadJokeService { get; set; }
    public MondayQuotesService MondayQuotesService { get; set; }
    public IRedneckJokeService RedneckJokeService { get; set; }

    [Command("dadjoke")]
    [Alias("dj")]
    public async Task DadJoke()
    {
        var joke = await DadJokeService.GetDadJoke();
        await ReplyAsync(joke.Joke, messageReference: new MessageReference(Context.Message.Id));
    }

    [Command("monday")]
    public async Task MondayQuote()
    {
        var joke = await MondayQuotesService.GetQuote();
        await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
    }

    [Command("redneckjoke")]
    [Alias("redneck", "rn")]
    public async Task RedneckJoke()
    {
        var joke = await RedneckJokeService.GetQuote();
        await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
    }
}
