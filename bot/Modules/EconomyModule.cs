using System.Threading.Tasks;
using bot.Features.Database;
using Discord;
using Discord.Commands;

namespace bot.Modules;

public class EconomyModule : CustomModule<SocketCommandContext>
{
    public BotDataService BotDataService { get; set; }

    [Command("coins")]
    public async Task GetCoins(IUser user = null)
    {
        user ??= Context.User;
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var userData = BotDataService.GetLevelData(guildId, userId);
        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .AddField("UserName", $"{user.Username}#{user.Discriminator}")
            .AddField("Balance", $"{userData.money} :coin:")
            .Build();
        await ReplyAsync(embed: embed);
    }

    [Command("addcoins")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task AddMoney(IUser user, ulong amount)
    {
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var d = BotDataService.AddMoney(guildId, userId, amount);
        await ReplyAsync($"Added **{amount}** :coin: to **{user.Username}**. Their new balance is **{d.money}** :coin:");
    }

    [Command("removecoins")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task RemoveMoney(IUser user, ulong amount)
    {
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var d = BotDataService.RemoveMoney(guildId, userId, amount);
        await ReplyAsync($"Removed **{amount}** :coin: from **{user.Username}**. Their new balance is: **{d.money}** :coin:");
    }
}
