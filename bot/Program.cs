using System;
using System.IO;
using bot.Configuration.Models;
using Microsoft.Extensions.DependencyInjection;
using bot.Features.DadJokes;
using bot.Features.Database;
using bot.Features.MondayQuotes;
using bot.Features.Pictures;
using bot.Features.RedneckJokes;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using bot.Features.Games;
using bot;
using Microsoft.AspNetCore.Builder;
using bot.Services;
using MediatR;
using FluentValidation;
using bot.PipelineBehaviors;
using bot.Extensions;
using Victoria;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("hostsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables(prefix: "BOT_");
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddCommandLine(args);

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("vic10usapi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["vic10usApi:BaseUrl"]);
});

//builder.Services.AddJokes(builder.Configuration);

builder.Services.AddDbContext<BotDbContext>
    (x => x.UseSqlite(builder.Configuration.GetConnectionString("BotDb")), ServiceLifetime.Singleton);
builder.Services.AddSingleton<DiceGame>();
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<CommandHandlingService>();
builder.Services.AddSingleton<PictureService>();
builder.Services.AddSingleton<DadJokeService>();
builder.Services.AddTransient<Program>();
builder.Services.AddSingleton<MondayQuotesService>();
builder.Services.AddSingleton<IRedneckJokeService, RedneckJokeService>();
builder.Services.AddSingleton<BotDataService>();
builder.Services.AddHttpClient<DadJokeService>("DadJokeService", (s, c) =>
{
    c.BaseAddress = new Uri(builder.Configuration["DadJokes:BaseUrl"]);
});
builder.Services.AddHostedService<LifetimeEventsHostedService>();
builder.Services.AddTransient<ImageApiService>();
builder.Services.Configure<DiscordBotDatabaseSettings>(
    builder.Configuration.GetSection(nameof(DiscordBotDatabaseSettings)));

builder.Services.AddSingleton<IDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<DiscordBotDatabaseSettings>>().Value);

builder.Services.AddMediatR(typeof(Program));
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddLavaNode(config => {
    config.SelfDeaf = true;
});

var app = builder.Build();

app.UseFluentValidationExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();