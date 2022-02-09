using System;
using System.IO;
using bot.Configuration.Models;
using Microsoft.Extensions.DependencyInjection;
using bot.Features.DadJokes;
using bot.Features.Database;
using bot.Features.MondayQuotes;
using bot.Features.Pictures;
using bot.Features.RedneckJokes;
using bot.Services.vic10usAPI;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using bot.Features.Games;

namespace bot;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(configHost =>
            {
                configHost
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("hostsettings.json", optional: true)
                    .AddEnvironmentVariables(prefix: "BOT_")
                    .AddUserSecrets<Program>(true, true)
                    .AddCommandLine(args);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration;
                services.AddHttpClient();
                services.AddHttpClient("vic10usapi", c =>
                {
                    c.BaseAddress = new Uri(config["vic10usApi:BaseUrl"]);
                });
                services.AddDbContext<BotDbContext>
                    (x => x.UseSqlite(hostContext.Configuration.GetConnectionString("BotDb")));
                services.AddSingleton<DiceGame>();
                services.AddSingleton<DiscordSocketClient>();
                services.AddSingleton<CommandService>();
                services.AddSingleton<CommandHandlingService>();
                services.AddSingleton<PictureService>();
                services.AddSingleton<DadJokeService>();
                services.AddSingleton(new SomeServiceClass());
                services.AddTransient<Program>();
                services.AddSingleton<MondayQuotesService>();
                services.AddSingleton<RedneckJokeService>();
                services.AddSingleton<BotDataService>();
                services.AddHttpClient<DadJokeService>("DadJokeService", (s, c) =>
                {
                        // var config = s.GetRequiredService<IConfiguration>();
                        c.BaseAddress = new Uri(config["DadJokes:BaseUrl"]);
                });
                services.AddHostedService<LifetimeEventsHostedService>();
                services.AddTransient<Vic10UsApiService>();
                services.Configure<DiscordBotDatabaseSettings>(
                    hostContext.Configuration.GetSection(nameof(DiscordBotDatabaseSettings)));

                services.AddSingleton<IDatabaseSettings>(sp =>
                    sp.GetRequiredService<IOptions<DiscordBotDatabaseSettings>>().Value);
            })
            .UseConsoleLifetime();
}
