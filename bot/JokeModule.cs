using System.Threading.Tasks;
using bot.Features.DadJokes;
using bot.Features.MondayQuotes;
using bot.Features.RedneckJokes;
using Discord;
using Discord.Commands;

namespace bot;

[Group("joke")]
public class JokeModule : ModuleBase<SocketCommandContext>
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public DadJokeService DadJokeService { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public MondayQuotesService MondayQuotesService { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public RedneckJokeService RedneckJokeService { get; set; }

    [Command("dad")]
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

    [Command("redneck")]
    [Alias("rn")]
    public async Task RedneckJoke()
    {
        var joke = await RedneckJokeService.GetQuote();
        await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
    }

    [Command]
    public async Task Help()
    {
        await ReplyAsync("use !joke [dad, redneck, monday]", messageReference: new MessageReference(Context.Message.Id));
    }
}
