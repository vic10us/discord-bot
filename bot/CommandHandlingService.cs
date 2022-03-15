using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using v10.Data.MongoDB;

namespace bot;

public class CommandHandlingService
{
  private readonly CommandService _commands;
  private readonly DiscordSocketClient _discord;
  private readonly IServiceProvider _services;
  private readonly BotDataService _botDataService;

  public CommandHandlingService(IServiceProvider services)
  {
    _commands = services.GetRequiredService<CommandService>();
    _discord = services.GetRequiredService<DiscordSocketClient>();
    _botDataService = services.GetRequiredService<BotDataService>();
    _services = services;

    // Hook CommandExecuted to handle post-command-execution logic.
    _commands.CommandExecuted += CommandExecutedAsync;
    // Hook MessageReceived so we can process each message to see
    // if it qualifies as a command.
    _discord.MessageReceived += MessageReceivedAsync;
  }

  public async Task InitializeAsync()
  {
    // Register modules that are public and inherit ModuleBase<T>.
    await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
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

    var context = new SocketCommandContext(_discord, message);
    HandleUserMessageEvents(context, argPos);
    if (!message.HasCharPrefix('!', ref argPos)) return;
    // Perform the execution of the command. In this method,
    // the command service will perform precondition and parsing check
    // then execute the command if one is matched.
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

  private static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
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
