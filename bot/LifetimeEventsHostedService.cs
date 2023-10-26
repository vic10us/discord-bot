using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;
using Random = System.Random;
using bot.Features.NaturalLanguageProcessing;
using System.Text.RegularExpressions;
using MediatR;
using v10.Bot.Discord;
using StackExchange.Redis;
using v10.Events.Core.Enums;
using v10.Events.Core.Commands;
using AspNetCoreRateLimit;

namespace bot;

internal class LifetimeEventsHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly CommandHandlingService _commandHandlingService;
    private readonly IConfiguration _config;
    private readonly IBotDataService _botDataService;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly IDiscordMessageService _messageService;

    private readonly IDatabase _database;
    private static readonly string MachineName = $"{Environment.MachineName}{Guid.NewGuid()}";
    private static readonly RedisValue RedisValue = MachineName;

    public LifetimeEventsHostedService(
        ILogger<LifetimeEventsHostedService> logger,
        IHostApplicationLifetime appLifetime,
        DiscordSocketClient discordSocketClient,
        CommandHandlingService commandHandlingService,
        IConfiguration config,
        IBotDataService botDataService,
        InteractionService interactions,
        IServiceProvider serviceProvider,
        IMediator mediator,
        IDiscordMessageService messageService)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _discordSocketClient = discordSocketClient;
        _commandHandlingService = commandHandlingService;
        _config = config;
        _botDataService = botDataService;
        _interactions = interactions;
        _serviceProvider = serviceProvider;
        _mediator = mediator;
        _messageService = messageService;
        var server = serviceProvider.GetRequiredService<IServer>();
        _database = server.Multiplexer.GetDatabase();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);
        _discordSocketClient.Log += LogAsync;
        _discordSocketClient.Ready += OnReadyAsync;
        _discordSocketClient.MessageReceived += _discordSocketClient_MessageReceived;
        _discordSocketClient.UserUpdated += _discordSocketClient_UserUpdated;
        _discordSocketClient.InviteCreated += _discordSocketClient_InviteCreated;
        _discordSocketClient.InviteDeleted += _discordSocketClient_InviteDeleted;
        _discordSocketClient.GuildAvailable += _discordSocketClient_GuildAvailable;
        _discordSocketClient.PresenceUpdated += _discordSocketClient_PresenceUpdated;
        _discordSocketClient.UserJoined += _discordSocketClient_UserJoined;
        _discordSocketClient.UserLeft += _discordSocketClient_UserLeft;

        await _discordSocketClient.LoginAsync(TokenType.Bot, _config["Discord:Token"]);
        await _discordSocketClient.StartAsync();

        _discordSocketClient.UserVoiceStateUpdated += DiscordSocketClient_UserVoiceStateUpdated;

        // Here we initialize the logic required to register our commands.
        await _commandHandlingService.InitializeAsync();
    }

    private async Task _discordSocketClient_UserJoined(SocketGuildUser user)
    {
        RedisKey key = $"_discordSocketClient_UserJoined_{user.Guild.Id}_{user.Id}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("User is already being processed {guild} {user}", user.Guild, user);
            return;
        }
        try
        {
            await ProcessUserJoined(user);
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
    }

    private async Task ProcessUserJoined(SocketGuildUser user)
    {
        await _messageService.SendMessageAsync(user.Guild.Id, "welcome.log", $"Welcome {user.Mention} to {user.Guild.Name}! Please read the rules and enjoy your stay!", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone));
    }

    private async Task _discordSocketClient_UserLeft(SocketGuild guild, SocketUser user)
    {
        RedisKey key = $"_discordSocketClient_UserLeft_{guild.Id}_{user.Id}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("User is already being processed {guild} {user}", guild, user);
            return;
        }
        try
        {
            await ProcessUserLeft(guild, user);
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
    }

    private async Task ProcessUserLeft(SocketGuild guild, SocketUser user)
    {
        await _messageService.SendMessageAsync(guild.Id, "goodbye.log", $"Goodbye {user.Mention} from {guild.Name}! We hope you enjoyed your stay!", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone));
    }

    private Task _discordSocketClient_MessageReceived(SocketMessage arg)
    {
        _logger.LogInformation("[Id: {Id}] Message received '{Content}'", arg.Id, arg.Content);
        
        RedisKey key = $"_discordSocketClient_MessageReceived_{arg.Id}";
        RedisValue token = Environment.MachineName;

        var lockTaken = _database.LockTake(key, token, TimeSpan.FromSeconds(10));
        if (!lockTaken)
        {
            _logger.LogWarning("Message is already being processed {Content}", arg.Content);
            return Task.CompletedTask;
        }

        try
        {
            return ProcessMessage(arg);
        }
        finally
        {
            _database.LockRelease(key, token);
        }
    }

    private Task ProcessMessage(SocketMessage arg) {
        if (arg.Author.IsBot)
            return Task.CompletedTask;

        var nlpService = _serviceProvider.GetService<INLPService>();
        var sentencesList = nlpService.GetSentences(arg.Content).ToList();

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
        RedisKey key = $"_discordSocketClient_PresenceUpdated_{arg1.Id}_{arg2.Status}_{arg3.Status}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("Presence is already being processed {arg1} {arg2} {arg3}", arg1, arg2, arg3);
            return Task.CompletedTask;
        }
        try
        {
            _logger.LogInformation("Presence updated {arg1} {arg2} {arg3}", arg1, arg2, arg3);
            return Task.CompletedTask;
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
    }

    private async Task _discordSocketClient_GuildAvailable(SocketGuild guild)
    {
        RedisKey key = $"_discordSocketClient_GuildAvailable_{guild.Id}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("Guild is already being processed {guild}", guild);
            return;
        }

        try
        {
            await ProcessGuildAvailable(guild);
            return;
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
        
    }

    private async Task ProcessGuildAvailable(SocketGuild guild)
    {
        _logger.LogInformation("Guild {guild} became available", guild);
        // var guildData = _botDataService.GetGuild(guild.Id);
        // if (guildData.guildName == null || guildData.guildName.Equals(guild.Name)) return Task.CompletedTask;
        await _mediator.Send(new UpdateGuildNameCommand()
        {
            GuildId = $"{guild.Id}",
            GuildName = guild.Name
        });
    }

    private Task _discordSocketClient_InviteDeleted(SocketGuildChannel arg1, string arg2)
    {
        RedisKey key = $"_discordSocketClient_InviteDeleted_{arg1.Id}_{arg2}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("Invite is already being processed {arg1} {arg2}", arg1, arg2);
            return Task.CompletedTask;
        }
        try
        {
            ProcessInviteDeleted(arg1, arg2);
            return Task.CompletedTask;
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
    }

    private void ProcessInviteDeleted(SocketGuildChannel arg1, string arg2)
    {
        _logger.LogInformation("Invite Deleted {arg1} {arg2}", arg1, arg2);
    }

    private Task _discordSocketClient_InviteCreated(SocketInvite arg)
    {
        RedisKey key = $"_discordSocketClient_InviteCreated_{arg.Code}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("Invite is already being processed {arg}", arg);
            return Task.CompletedTask;
        }
        try
        {
            ProcessInviteCreated(arg);
            return Task.CompletedTask;
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
    }

    private void ProcessInviteCreated(SocketInvite arg)
    {
        var who = $"{arg.Inviter.Username}#{arg.Inviter.Discriminator}".TrimEnd('#');
        var whoId = $"{arg.Inviter.Id}";
        _logger.LogInformation("Invite Created by {who}({whoId}) [{arg}]", who, whoId, arg);

    }

    private Task _discordSocketClient_UserUpdated(SocketUser arg1, SocketUser arg2)
    {
        RedisKey key = $"_discordSocketClient_UserUpdated_{arg1.Id}_{arg2.Id}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("User is already being processed {arg1} {arg2}", arg1, arg2);
            return Task.CompletedTask;
        }
        try
        {
            ProcessUserUpdated(arg1, arg2);
            return Task.CompletedTask;
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
    }

    private void ProcessUserUpdated(SocketUser arg1, SocketUser arg2)
    {
        _logger.LogInformation("User updated Before {arg1} After {arg2}", arg1, arg2);
    }

    private Task _discordSocketClient_ButtonExecuted(SocketMessageComponent arg)
    {
        RedisKey key = $"_discordSocketClient_ButtonExecuted_{arg.Id}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("Button is already being processed {arg}", arg);
            return Task.CompletedTask;
        }
        try
        {
            ProcessButtonExecuted(arg);
            return Task.CompletedTask;
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
    }

    private void ProcessButtonExecuted(SocketMessageComponent arg)
    {
        _logger.LogInformation("Button was clicked");
    }

    private async Task OnReadyAsync()
    {
        Console.WriteLine("Bot is connected!");

        await UpdateGuildCommands();
    }

    private async Task UpdateGuildCommands()
    {
        _discordSocketClient.Guilds.ToList().ForEach(c => _logger.LogInformation(c.Name));
        // var guild = _discordSocketClient.GetGuild(761581939697254431);
        foreach (var guild in _discordSocketClient.Guilds)
        {
            RedisKey key = $"UpdateGuildCommands_{guild.Id}";
            if (!_database.LockTake(key, RedisValue, TimeSpan.FromMinutes(5)))
            {
                _logger.LogWarning("Guild is already being processed {guild}", guild);
                continue;
            }
            try
            {
                await _interactions.RegisterCommandsToGuildAsync(guild.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guild commands {guild}", guild);
                _database.LockRelease(key, RedisValue);
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

    private async Task DiscordSocketClient_UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        RedisKey key = $"DiscordSocketClient_UserVoiceStateUpdated_{user.Id}";
        if (!_database.LockTake(key, RedisValue, TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("User is already being processed {user}", user);
            return;
        }

        try
        {
            await ProcessUserVoiceStateUpdated(user, before, after);
        }
        finally
        {
            _database.LockRelease(key, RedisValue);
        }
    }

    private async Task ProcessUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
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
            await _messageService.SendMessageAsync(guildId, "system.log", m);
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
                _mediator.Send(new UserLevelChangedCommand()
                {
                    GuildId = guildId,
                    UserId = userId,
                    NewLevel = newLevel,
                    Type = XpType.Voice
                });
                // SendMessageAsync(guildId, "level.log", $"Congratulations {user.Mention}, you leveled up your Voice Level!!! New Voice Level: {newLevel}").Wait();
            });
            var m = $"{user} Left voice in {before} [Server: {before.VoiceChannel.Guild}] and gained {xp}xp in the process [Time: {minutesInVc} minutes]";
            await _messageService.SendMessageAsync(guildId, "system.log", m);
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
            await _messageService.SendMessageAsync(guildId, "system.log", m);
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
