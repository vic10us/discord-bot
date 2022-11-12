using System;
using System.Reflection;
using System.Threading.Tasks;
using bot.Features.Metrics;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using v10.Data.MongoDB;

namespace bot;

public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly ILogger<CommandHandlingService> _logger;
    private readonly BotDataService _botDataService;

    public CommandHandlingService(IServiceProvider services, ILogger<CommandHandlingService> logger)
    {
        _commands = services.GetRequiredService<CommandService>();
        _interactions = services.GetRequiredService<InteractionService>();
        _client = services.GetRequiredService<DiscordSocketClient>();
        _botDataService = services.GetRequiredService<BotDataService>();
        _services = services;
        _logger = logger;


        // Hook CommandExecuted to handle post-command-execution logic.
        _commands.CommandExecuted += CommandExecutedAsync;
        // Hook MessageReceived so we can process each message to see
        // if it qualifies as a command.
        _client.MessageReceived += MessageReceivedAsync;
    }

    public async Task InitializeAsync()
    {
        // Register modules that are public and inherit ModuleBase<T>.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // Process the InteractionCreated payloads to execute Interactions commands
        _client.InteractionCreated += HandleInteraction;

        // Process the command execution results 
        _interactions.SlashCommandExecuted += SlashCommandExecuted;
        _interactions.ContextCommandExecuted += ContextCommandExecuted;
        _interactions.ComponentCommandExecuted += ComponentCommandExecuted;
        _interactions.ModalCommandExecuted += ModalCommandExecuted;
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_client, arg);
            var result = await _interactions.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task ModalCommandExecuted(ModalCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (rawMessage is not SocketUserMessage { Source: MessageSource.User } message) return;

        // This value holds the offset where the prefix ends
        var argPos = 0;
        // Perform prefix check. You may want to replace this with
        // (!message.HasCharPrefix('!', ref argPos))
        // for a more traditional command format like !help.
        //if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;

        var context = new SocketCommandContext(_client, message);
        HandleUserMessageEvents(context, argPos);
        if (!message.HasCharPrefix('!', ref argPos)) return;
        // Perform the execution of the command. In this method,
        // the command service will perform precondition and parsing check
        // then execute the command if one is matched.
        // TelemetryTools.s_botCommandsHandled.Add(1);
        TelemetryTools.BotCommandHandled(message.Content.Substring(argPos).Split(" ")[0]);
        await _commands.ExecuteAsync(context, argPos, _services);
        // Note that normally a result will be returned by this format, but here
        // we will handle the result in CommandExecutedAsync,
    }

    protected virtual async Task<IUserMessage> ReplyAsync(
        ICommandContext context,
        string message = null,
        bool isTTS = false,
        Embed embed = null,
        RequestOptions options = null,
        AllowedMentions allowedMentions = null,
        MessageReference messageReference = null)
    {
        return await context.Channel.SendMessageAsync(message, isTTS, embed, options, allowedMentions, messageReference).ConfigureAwait(false);
    }

    private void HandleUserMessageEvents(ICommandContext context, int argPos)
    {
        var userId = context.User.Id;
        var guildId = context.Guild?.Id ?? 0;
        if (context.Message.HasCharPrefix('!', ref argPos)) return;
        _botDataService.IncrementUserMessageCount(guildId, userId);
        var userLevel = _botDataService.GetLevelData(guildId, userId);
        if ((DateTimeOffset.UtcNow - userLevel.lastUpdated) < TimeSpan.FromMinutes(1)) return;
        var xp = (ulong)new Random().Next(15, 20);
        _botDataService.AddXp(guildId, userId, xp,
            (newLevel) =>
            {
                ReplyAsync(context, $"Congratulations, you leveled up!!! New Level: {newLevel}",
                    messageReference: new MessageReference(context.Message.Id)).Wait();
            });
    }

    private static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
    {
        // command is unspecified when there was a search failure (command not found); we don't care about these errors
        if (!command.IsSpecified)
            return;

        // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
        if (result.IsSuccess)
            return;

        // the command failed, let's notify the user that something happened.
        await context.Channel.SendMessageAsync($"error: {result}");
    }
}
