using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bot.Features.Database;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;
using Victoria.Node;
using edu.stanford.nlp.pipeline;
using Random = System.Random;
using bot.Features.NaturalLanguageProcessing;
using System.Text.RegularExpressions;
using bot.Commands;
using MediatR;
using bot.Modules;

namespace bot;

internal class LifetimeEventsHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly CommandHandlingService _commandHandlingService;
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LavaNode _lavaNode;
    private readonly BotDataService _botDataService;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;

    public LifetimeEventsHostedService(
        ILogger<LifetimeEventsHostedService> logger,
        IHostApplicationLifetime appLifetime,
        DiscordSocketClient discordSocketClient,
        CommandHandlingService commandHandlingService,
        IConfiguration config,
        BotDataService botDataService,
        IServiceScopeFactory scopeFactory,
        InteractionService interactions,
        IServiceProvider serviceProvider,
        IMediator mediator
        )
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _discordSocketClient = discordSocketClient;
        _commandHandlingService = commandHandlingService;
        _config = config;
        _scopeFactory = scopeFactory;
        _botDataService = botDataService;
        _interactions = interactions;
        _serviceProvider = serviceProvider;
        _mediator = mediator;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);

        //using (var scope = _scopeFactory.CreateScope())
        //{
        //    var context = scope.ServiceProvider.GetRequiredService<BotDbContext>();
        //    await context.Database.EnsureCreatedAsync(cancellationToken);
        //}

        _discordSocketClient.Log += LogAsync;
        _discordSocketClient.Ready += OnReadyAsync;
        _discordSocketClient.MessageReceived += _discordSocketClient_MessageReceived;
        _discordSocketClient.UserUpdated += _discordSocketClient_UserUpdated;
        _discordSocketClient.InviteCreated += _discordSocketClient_InviteCreated;
        _discordSocketClient.InviteDeleted += _discordSocketClient_InviteDeleted;
        _discordSocketClient.GuildAvailable += _discordSocketClient_GuildAvailable;
        _discordSocketClient.PresenceUpdated += _discordSocketClient_PresenceUpdated;
        // _commandService.Log += LogAsync;
        // _discordSocketClient.ButtonExecuted += _discordSocketClient_ButtonExecuted;

        await _discordSocketClient.LoginAsync(TokenType.Bot, _config["Discord:Token"]);
        await _discordSocketClient.StartAsync();

        _discordSocketClient.UserVoiceStateUpdated += DiscordSocketClient_UserVoiceStateUpdated;

        // Here we initialize the logic required to register our commands.
        await _commandHandlingService.InitializeAsync();
    }

    private Task _discordSocketClient_MessageReceived(SocketMessage arg)
    {
        _logger.LogInformation("Message received {Content}", arg.Content);
        
        if (arg.Author.IsBot)
            return Task.CompletedTask;

        var nlpService = _serviceProvider.GetService<INLPService>();
        var sentencesList = nlpService.GetSentences(arg.Content);

        // a case insensitive regular expression that detects "I'm", "im" or "I am" and extracts the remainder of the sentence.
        var regex = new Regex(@"\b(i'm|im|i am)\s(.*?)(?=[\.,\!?\n]|$)", RegexOptions.IgnoreCase);

        sentencesList.Where(sentence => regex.IsMatch(sentence))
            .ToList()
            .ForEach(async sentence =>
            {
                var match = regex.Match(sentence);
                var who = match.Groups[2].Value.Trim();
                var response = $"Hi {who}, I'm dad!";
                var messageReference = new MessageReference(arg.Id);
                await arg.Channel.SendMessageAsync(response, false, null, null, null, messageReference);
            });

        return Task.CompletedTask;
    }

    private Task _discordSocketClient_PresenceUpdated(SocketUser arg1, SocketPresence arg2, SocketPresence arg3)
    {
        _logger.LogInformation("Presence updated {arg1} {arg2} {arg3}", arg1, arg2, arg3);
        return Task.CompletedTask;
    }

    private Task _discordSocketClient_GuildAvailable(SocketGuild guild)
    {
        _logger.LogInformation("Guild {guild} became available", guild);
        return Task.CompletedTask;
    }

    private Task _discordSocketClient_InviteDeleted(SocketGuildChannel arg1, string arg2)
    {
        _logger.LogInformation("Invite Deleted {arg1} {arg2}", arg1, arg2);
        return Task.CompletedTask;
    }

    private Task _discordSocketClient_InviteCreated(SocketInvite arg)
    {
        var who = $"{arg.Inviter.Username}#{arg.Inviter.Discriminator}".TrimEnd('#');
        var whoId = $"{arg.Inviter.Id}";
        _logger.LogInformation("Invite Created by {who}({whoId}) [{arg}]", who, whoId, arg);
        return Task.CompletedTask;
    }

    private Task _discordSocketClient_UserUpdated(SocketUser arg1, SocketUser arg2)
    {
        _logger.LogInformation("User updated Before {arg1} After {arg2}", arg1, arg2);
        return Task.CompletedTask;
    }

    private Task _discordSocketClient_ButtonExecuted(SocketMessageComponent arg)
    {
        _logger.LogInformation("Button was clicked", arg);
        return Task.CompletedTask;
    }

    private async Task OnReadyAsync()
    {
        Console.WriteLine("Bot is connected!");

        if (_lavaNode != null && !_lavaNode.IsConnected)
        {
            await _lavaNode.ConnectAsync();
        }

        // await _interactions.RegisterCommandsGloballyAsync(true);
        _discordSocketClient.Guilds.ToList().ForEach(c => _logger.LogInformation(c.Name));
        // var guild = _discordSocketClient.GetGuild(761581939697254431);
        foreach (var guild in _discordSocketClient.Guilds)
        {
            try
            {
                //var commands = await guild.GetApplicationCommandsAsync();
                await _interactions.RegisterCommandsToGuildAsync(guild.Id);

                //// var commands = await _discordSocketClient.GetGlobalApplicationCommandsAsync();
                //foreach (var command in commands)
                //{
                //    _logger.LogInformation($"slash command {command.Name}");
                //    await command.DeleteAsync();
                //}
            }
            catch
            {
                // do nothing for now
            }
        }
    }

    private static double GetMinutesInVoice(UserVoiceStats userVoiceStats)
    {
        if (userVoiceStats == null) return 0;
        if (userVoiceStats.channelId.Equals(string.Empty)) return 0;
        if (userVoiceStats.lastJoinedAt.Equals(DateTime.MinValue)) return 0;
        if (userVoiceStats.lastJoinedAt >= DateTimeOffset.Now) return 0;
        var diff = userVoiceStats.lastExitedAt - userVoiceStats.lastJoinedAt;
        var result = diff.TotalMinutes;
        return result;
    }

    private async Task SendMessageAsync(ulong guildId, string route, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
    {
        var guildData = _botDataService.GetGuild(guildId);
        if (guildData == null) return;
        if (!guildData.channelNotifications.ContainsKey(route)) return;
        var channelId_str = guildData.channelNotifications[route];
        if (string.IsNullOrWhiteSpace(channelId_str)) return;
        if (!ulong.TryParse(channelId_str, out var channelId)) return;
        await SendMessageAsync(channelId, message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
    }

    private async Task SendMessageAsync(ulong channelId, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
    {
        var channel = _discordSocketClient.GetChannel(channelId);
        await (channel as IMessageChannel)?.SendMessageAsync(message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
    }

    private async Task DiscordSocketClient_UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        var userId = user.Id;
        if (before.VoiceChannel == null)
        {
            var m = $"{user} Joined voice in {after} [Server: {after.VoiceChannel.Guild}]";
            var guildId = after.VoiceChannel.Guild?.Id ?? 0;
            var voiceStats = _botDataService.GetUserVoiceStats(guildId, userId);
            voiceStats.channelId = $"{after.VoiceChannel.Id}";
            voiceStats.lastJoinedAt = DateTimeOffset.Now;
            voiceStats.isActive = true;
            _botDataService.UpdateUserVoiceStats(voiceStats);
            await SendMessageAsync(guildId, "system.log", m);
            _logger.LogInformation(m);
        }
        else if (after.VoiceChannel == null)
        {
            var guildId = before.VoiceChannel.Guild?.Id ?? 0;
            var voiceStats = _botDataService.GetUserVoiceStats(guildId, userId);
            voiceStats.lastExitedAt = DateTimeOffset.Now;
            voiceStats.isActive = false;
            var minutesInVc = !voiceStats.channelId.Equals($"{before.VoiceChannel.Id}") ? (double)0 : GetMinutesInVoice(voiceStats);
            voiceStats.totalTimeSpentInVoice += (ulong)Math.Round(minutesInVc);
            voiceStats.lastJoinedAt = DateTimeOffset.MinValue;
            _botDataService.UpdateUserVoiceStats(voiceStats);
            var xp = (ulong)(new Random().Next(15, 20) * minutesInVc);
            _botDataService.AddVoiceXp(guildId, userId, xp, (newLevel) => {
                _mediator.Send(new UserLevelChangedCommand() { 
                    GuildId = guildId,
                    UserId = userId,
                    NewLevel = newLevel,
                    Type = XpType.Voice
                });
                // SendMessageAsync(guildId, "level.log", $"Congratulations {user.Mention}, you leveled up your Voice Level!!! New Voice Level: {newLevel}").Wait();
            });
            var m = $"{user} Left voice in {before} [Server: {before.VoiceChannel.Guild}] and gained {xp}xp in the process [Time: {minutesInVc} minutes]";
            await SendMessageAsync(guildId, "system.log", m);
            _logger.LogInformation(m);
        }
        else
        {
            if (before.VoiceChannel.Id == after.VoiceChannel.Id) return; // Status changed
            var guildId = after.VoiceChannel.Guild?.Id ?? 0;
            var voiceStats = _botDataService.GetUserVoiceStats(guildId, userId);
            voiceStats.channelId = $"{after.VoiceChannel.Id}";
            voiceStats.isActive = true;
            _botDataService.UpdateUserVoiceStats(voiceStats);
            var m = $"{user} Moved voice from {before} to {after} [Server: {after.VoiceChannel.Guild}]";
            await SendMessageAsync(guildId, "system.log", m);
            _logger.LogInformation(m);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        _logger.LogInformation("OnStarted has been called.");
        // var client = services.GetRequiredService<DiscordSocketClient>();

        // Perform post-startup activities here
    }

    private void OnStopping()
    {
        _logger.LogInformation("OnStopping has been called.");

        // Perform on-stopping activities here
    }

    private void OnStopped()
    {
        _logger.LogInformation("OnStopped has been called.");

        // Perform post-stopped activities here
    }

    private static Task LogAsync(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
        }
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();

        // If you get an error saying 'CompletedTask' doesn't exist,
        // your project is targeting .NET 4.5.2 or lower. You'll need
        // to adjust your project's target framework to 4.6 or higher
        // (instructions for this are easily Googled).
        // If you *need* to run on .NET 4.5 for compat/other reasons,
        // the alternative is to 'return Task.Delay(0);' instead.
        return Task.CompletedTask;
    }
}
