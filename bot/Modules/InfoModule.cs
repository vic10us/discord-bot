using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace bot.Modules;

// Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
public class InfoModule : CustomModule<SocketCommandContext>
{

    public InfoModule(
        IServiceProvider serviceProvider,
        ILogger<InfoModule> logger
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        _database = server.Multiplexer.GetDatabase();
        _logger = logger;
    }

    [Command("ping")]
    [Alias("pong", "hello")]
    public async Task PingAsync()
    {
        if (!EnsureSingle()) { return; }
        try
        {
            await ReplyAsync("pong!");
        }
        finally
        {
            ReleaseLock();
        }
    }
}
