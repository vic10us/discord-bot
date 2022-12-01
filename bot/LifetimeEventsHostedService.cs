using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bot.Features.Database;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;
using Victoria;
using Victoria.Node;

namespace bot;

internal class LifetimeEventsHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly CommandService _commandService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly CommandHandlingService _commandHandlingService;
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LavaNode _lavaNode;
    private readonly BotDataService _botDataService;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _serviceProvider;

    public LifetimeEventsHostedService(
        ILogger<LifetimeEventsHostedService> logger,
        IHostApplicationLifetime appLifetime,
        CommandService commandService,
        DiscordSocketClient discordSocketClient,
        CommandHandlingService commandHandlingService,
        IConfiguration config,
        BotDataService botDataService,
        IServiceScopeFactory scopeFactory,
        InteractionService interactions,
        IServiceProvider serviceProvider,
        LavaNode lavaNode)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _commandService = commandService;
        _discordSocketClient = discordSocketClient;
        _commandHandlingService = commandHandlingService;
        _config = config;
        _scopeFactory = scopeFactory;
        _lavaNode = lavaNode;
        _botDataService = botDataService;
        _interactions = interactions;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);

        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BotDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }

        _discordSocketClient.Log += LogAsync;
        _discordSocketClient.Ready += OnReadyAsync;
        // _commandService.Log += LogAsync;
        // _discordSocketClient.ButtonExecuted += _discordSocketClient_ButtonExecuted;

        await _discordSocketClient.LoginAsync(TokenType.Bot, _config["Discord:Token"]);
        await _discordSocketClient.StartAsync();
        _discordSocketClient.Ready += () =>
        {
            Console.WriteLine("Bot is connected!");
            return Task.CompletedTask;
        };

        _discordSocketClient.UserVoiceStateUpdated += DiscordSocketClient_UserVoiceStateUpdated;

        // Here we initialize the logic required to register our commands.
        await _commandHandlingService.InitializeAsync();
    }

    private async Task _discordSocketClient_ButtonExecuted(SocketMessageComponent arg)
    {
        _logger.LogInformation("Button was clicked", arg);
        //var ctx = new SocketInteractionContext<SocketMessageComponent>(_discordSocketClient, arg);
        //await _interactions.ExecuteCommandAsync(ctx, _serviceProvider);
    }

    private async Task OnReadyAsync()
    {
        if (!_lavaNode.IsConnected)
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

    private async Task SendMessageAsync(ulong guildId, string route, string message)
    {
        var guildData = _botDataService.GetGuild(guildId);
        if (guildData == null) return;
        if (!guildData.channelNotifications.ContainsKey(route)) return;
        var channelId_str = guildData.channelNotifications[route];
        if (string.IsNullOrWhiteSpace(channelId_str)) return;
        if (!ulong.TryParse(channelId_str, out var channelId)) return;
        await SendMessageAsync(channelId, message);
    }

    private async Task SendMessageAsync(ulong channelId, string message)
    {
        var channel = _discordSocketClient.GetChannel(channelId);
        await (channel as IMessageChannel)?.SendMessageAsync(message);
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
            _botDataService.AddXp(guildId, userId, xp);
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
