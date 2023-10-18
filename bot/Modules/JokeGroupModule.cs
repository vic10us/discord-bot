using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using v10.Services.DadJokes;
using v10.Services.MondayQuotes;
using v10.Services.RedneckJokes;

namespace bot.Modules;

[Group("joke")]
public class JokeGroupModule : CustomModule<SocketCommandContext>
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public IDadJokeService DadJokeService { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public IMondayQuotesService MondayQuotesService { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public IRedneckJokeService RedneckJokeService { get; set; }

    [Command("dad")]
    [Alias("dj")]
    public async Task DadJoke()
    {
        var joke = await DadJokeService.GetJokeAsync();
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
