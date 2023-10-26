using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Data.MongoDB;

namespace bot.Modules;

public class EconomyModule : CustomModule<SocketCommandContext>
{
    public IBotDataService BotDataService { get; set; }
    public ILogger<EconomyModule> Logger { get; set; }

    public EconomyModule(
        IServiceProvider serviceProvider,
        ILogger<EconomyModule> logger
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        _database = server.Multiplexer.GetDatabase();
    }

    [Command("balance")]
    [Alias("coins")]
    [Summary("Check your balance")]
    [Remarks("balance")]
    public async Task GetBalance()
    {
        if (!EnsureSingle()) { return; }
        var user = Context.User;
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var userBalanceResult = await BotDataService.GetUserBalance(guildId, userId);
        await userBalanceResult.Match(async r =>
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .AddField("UserName", $"{user.Username}#{user.Discriminator}")
                .AddField("Balance", $"{r} :coin:")
                .Build();
            await ReplyAsync(embed: embed);
        }, async e =>
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .AddField("UserName", $"{user.Username}#{user.Discriminator}")
                .AddField("Balance", $"{e.Message}")
                .Build();
            await ReplyAsync(embed: embed);
        });
        ReleaseLock();
    }

    [Command("leave")]
    [Summary("Leave the economy")]
    [Remarks("leave")]
    public async Task Leave(IUser user = null)
    {
        if (!EnsureSingle()) { return; }
        // LeaveEconomy
        user ??= Context.User;
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var result = await BotDataService.LeaveEconomy(guildId, userId);
        await result.Match(async r =>
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("You have left the economy!")
                .AddField("User", $"{user.Username}#{user.Discriminator}")
                .Build();
            await ReplyAsync(embed: embed);
        }, async e =>
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Failed to leave the economy!")
                .AddField("User", $"{user.Username}#{user.Discriminator}")
                .AddField("Error", $"{e.Message}")
                .Build();
            await ReplyAsync(embed: embed);
        });
        ReleaseLock();
    }

    [Command("join")]
    [Summary("Join the economy")]
    [Remarks("join")]
    public async Task Join(IUser user = null)
    {
        if (!EnsureSingle()) { return; }
        user ??= Context.User;
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var result = await BotDataService.JoinEconomy(guildId, userId);
        await result.Match(async r =>
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Welcome to the economy!")
                .AddField("User", $"{user.Username}#{user.Discriminator}")
                .AddField("Balance", $"{r} :coin:")
                .Build();
            await ReplyAsync(embed: embed);
        }, async e =>
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Could not join the economy!")
                .AddField("User", $"{user.Username}#{user.Discriminator}")
                .AddField("Error", $"{e.Message}")
                .Build();
            await ReplyAsync(embed: embed);
        });
        ReleaseLock();
    }

    [Command("coins")]
    public async Task GetCoins(IUser user = null)
    {
        if (!EnsureSingle()) { return; }
        user ??= Context.User;
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var userData = BotDataService.GetLevelData(guildId, userId);
        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .AddField("User", $"{user.Username}#{user.Discriminator}")
            .AddField("Balance", $"{userData.money} :coin:")
            .Build();
        await ReplyAsync(embed: embed);
        ReleaseLock();
    }

    [Command("addcoins")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task AddMoney(IUser user, ulong amount)
    {
        if (!EnsureSingle()) { return; }
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var d = BotDataService.AddMoney(guildId, userId, amount);
        await ReplyAsync($"Added **{amount}** :coin: to **{user.Username}**. Their new balance is **{d.money}** :coin:");
        ReleaseLock();
    }

    [Command("removecoins")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task RemoveMoney(IUser user, ulong amount)
    {
        if (!EnsureSingle()) { return; }
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var d = BotDataService.RemoveMoney(guildId, userId, amount);
        await ReplyAsync($"Removed **{amount}** :coin: from **{user.Username}**. Their new balance is: **{d.money}** :coin:");
        ReleaseLock();
    }
}
