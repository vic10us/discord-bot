﻿using System.Threading.Tasks;
using bot.Features.DadJokes;
using bot.Features.MondayQuotes;
using bot.Features.RedneckJokes;
using Discord.Commands;

namespace bot.Modules;

// Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
public class InfoModule : CustomModule<SocketCommandContext>
{
    [Command("ping")]
    [Alias("pong", "hello")]
    public Task PingAsync()
        => ReplyAsync("pong!");

    //// [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
    //[Command("echo")]
    //public Task EchoAsync([Remainder] string text)
    //    // Insert a ZWSP before the text to prevent triggering other bots!
    //    => ReplyAsync('\u200B' + text, messageReference: new MessageReference(Context.Message.Id));

    //// 'params' will parse space-separated elements into a list
    //[Command("list")]
    //public Task ListAsync(params string[] objects)
    //    => ReplyAsync("You listed: " + string.Join("; ", objects));

    //// Setting a custom ErrorMessage property will help clarify the precondition error
    //[Command("guild_only")]
    //[RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
    //public Task GuildOnlyCommand()
    //    => ReplyAsync("Nothing to see here!");

    //// ~say hello world -> hello world
    //[Command("say")]
    //[Summary("Echoes a message.")]
    //public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
    //    => ReplyAsync(echo);
}
