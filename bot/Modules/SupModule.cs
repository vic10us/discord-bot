using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace bot.Modules;

[Group("sup")]
public class SupModule : CustomModule<SocketCommandContext>
{
    public SupModule(
        IServiceProvider serviceProvider,
        ILogger<SupModule> logger
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        _database = server.Multiplexer.GetDatabase();
        _logger = logger;
    }

    // ~sample square 20 -> 400
    [Command("square")]
    [Summary("Squares a number.")]
    public async Task SquareAsync(
        [Summary("The number to square.")]
            int num)
    {
        if (!EnsureSingle()) { return; }
        try
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }
        finally
        {
            ReleaseLock();
        }
    }

    // ~sample userinfo --> foxbot#0282
    // ~sample userinfo @Khionu --> Khionu#8708
    // ~sample userinfo Khionu#8708 --> Khionu#8708
    // ~sample userinfo Khionu --> Khionu#8708
    // ~sample userinfo 96642168176807936 --> Khionu#8708
    // ~sample whois 96642168176807936 --> Khionu#8708
    [Command("userinfo")]
    [Summary("Returns info about the current user, or the user parameter, if one passed.")]
    [Alias("user", "whois")]
    public async Task UserInfoAsync(
        [Summary("The (optional) user to get info from")]
            SocketUser user = null)
    {
        if (!EnsureSingle()) { return; }
        try
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
        finally
        {
            ReleaseLock();
        }
    }
}
