using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using bot.Features.FeatureManagement;
using bot.Features.Metrics;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using StackExchange.Redis;
using v10.Data.MongoDB;
using v10.Events.Core.Commands;
using v10.Events.Core.Enums;

namespace bot;

public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly ILogger<CommandHandlingService> _logger;
    private readonly IBotDataService _botDataService;
    private readonly IMediator _mediator;

    private readonly IDatabase _database;
    private static readonly string MachineName = $"{Environment.MachineName}{Guid.NewGuid()}";
    private static readonly RedisValue RedisValue = MachineName;

    public CommandHandlingService(IServiceProvider services, ILogger<CommandHandlingService> logger, IMediator mediator)
    {
        _commands = services.GetRequiredService<CommandService>();
        _interactions = services.GetRequiredService<InteractionService>();
        _client = services.GetRequiredService<DiscordSocketClient>();
        _botDataService = services.GetRequiredService<IBotDataService>();
        _services = services;
        _logger = logger;

        // Hook CommandExecuted to handle post-command-execution logic.
        _commands.CommandExecuted += CommandExecutedAsync;
        // Hook MessageReceived so we can process each message to see
        // if it qualifies as a command.
        _client.MessageReceived += MessageReceivedAsync;
        _mediator = mediator;

        var server = services.GetRequiredService<IServer>();
        _database = server.Multiplexer.GetDatabase();
    }

    protected internal IEnumerable<Type> GetEnabledModules()
    {
        var featureManager = _services.GetService<IFeatureManager>();

        var types = Assembly.GetEntryAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => typeof(IModuleBase).IsAssignableFrom(i)));
        
        return types.Where(t =>
        {
            if (t.GetCustomAttributes(typeof(FeatureModuleGateAttribute), true).FirstOrDefault() is not FeatureModuleGateAttribute hasFeatureGate) return true;
            return hasFeatureGate.Features.Any(feature => featureManager.IsEnabledAsync(feature).GetAwaiter().GetResult());
        });
    }

    protected internal async Task AddEnabledModulesAsync() 
    {
        var enabledModules = GetEnabledModules();
        foreach (var module in enabledModules)
        {
            await _commands.AddModuleAsync(module, _services);
        }
    }

    public async Task InitializeAsync()
    {
        // Register modules that are public and inherit ModuleBase<T>.
        // await _commands.AddModuleAsync<MusicModule>(_services);
        await AddEnabledModulesAsync();

        // await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
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

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task ModalCommandExecuted(ModalCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        RedisKey redisKey = $"bot:messages_{rawMessage.Id}";
        if (!_database.LockTake(redisKey, RedisValue, TimeSpan.FromSeconds(1)))
        {
            var machineName = _database.LockQuery(redisKey);
            _logger.LogWarning("Message {rawMessageId} already being processed by {machineName}", rawMessage.Id, machineName);
            return;
        }

        try
        {
            await HandleMessageAsync(rawMessage);
        }
        finally
        {
            _database.LockRelease(redisKey, RedisValue);
        }
    }

    private async Task HandleMessageAsync(SocketMessage rawMessage)
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
            (newLevel) => {
                _mediator.Send(new UserLevelChangedCommand
                {
                    GuildId = guildId,
                    UserId = userId,
                    NewLevel = newLevel,
                    Type = XpType.Text
                });
            });
    }

    private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
    {
        RedisKey redisKey = $"bot:commands_{context.Message.Id}";
        if (!_database.LockTake(redisKey, RedisValue, TimeSpan.FromSeconds(1)))
        {
            var machineName = _database.LockQuery(redisKey);
            _logger.LogWarning("Command {commandName} already being processed by {machineName}", command.Value.Name, machineName);
            return;
        }

        try 
        {
            await HandleCommandAsync(command, context, result);
        }
        finally
        {
            _database.LockRelease(redisKey, RedisValue);
        }
    }

    private static async Task HandleCommandAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
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
