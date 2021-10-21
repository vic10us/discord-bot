using System;
using System.Threading;
using System.Threading.Tasks;
using bot.Features.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace bot
{
    internal class LifetimeEventsHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly CommandHandlingService _commandHandlingService;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;

        public LifetimeEventsHostedService(
            ILogger<LifetimeEventsHostedService> logger, 
            IHostApplicationLifetime appLifetime,
            CommandService commandService,
            DiscordSocketClient discordSocketClient,
            CommandHandlingService commandHandlingService,
            IConfiguration config,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _commandService = commandService;
            _discordSocketClient = discordSocketClient;
            _commandHandlingService = commandHandlingService;
            _config = config;
            _scopeFactory = scopeFactory;
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
            _commandService.Log += LogAsync;

            await _discordSocketClient.LoginAsync(TokenType.Bot, _config["Discord:Token"]);
            await _discordSocketClient.StartAsync();
            _discordSocketClient.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };

            // Here we initialize the logic required to register our commands.
            await _commandHandlingService.InitializeAsync();
            
            // return Task.CompletedTask;
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
}