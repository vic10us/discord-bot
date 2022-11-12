using System;
using System.Linq;
using System.Threading.Tasks;
using bot.Features.Games;
using Discord;
using Discord.Commands;
using v10.Data.MongoDB;

namespace bot.Modules;

public class GamesModule : CustomModule<SocketCommandContext>
{
    public BotDataService BotDataService { get; set; }
    public DiceGame DiceGame { get; set; }

    [Command("dicegame")]
    public async Task RollDice(uint betAmount = 0)
    {
        if (betAmount == 0 || betAmount > 240)
        {
            await ReplyAsync("You must provide an amount to bet up to a max amount of 240",
                messageReference: new MessageReference(Context.Message.Id));
            return;
        }
        var guildId = Context.Guild?.Id ?? 0;
        var userId = Context.User.Id;
        var m = await BotDataService.GetMessageThrottle(guildId, userId, "dicegame");
        if (m != null)
        {
            var waitUntil = m.expiry.ToLocalTime() - DateTimeOffset.UtcNow;
            await ReplyAsync($"Whoa! Hold your horses. You need to wait {waitUntil.TotalMinutes} minutes to play again.");
            return;
        }
        _ = await BotDataService.GetMessageThrottle(guildId, userId, "dicegame", true);
        var userData = BotDataService.GetLevelData(guildId, userId);
        //TODO: Implement cooldown timer
        if (userData.money < betAmount)
        {
            await ReplyAsync($":game_die: **{Context.User.Username}**, you don't have enough coins to gamble this amount",
                    messageReference: new MessageReference(Context.Message.Id));
            return;
        }

        await ReplyAsync($":game_die: **{Context.User.Username}** bets **{betAmount}** :coin: and throws their dice...");
        await Task.Delay(1000);
        var rolls = DiceGame.GetNextRolls(6, 2);
        await ReplyAsync($":game_die: **{Context.User.Username}** gets **{rolls[0]}** and **{rolls[1]}**...");
        if (rolls[0] == 6 && rolls[0] == rolls[1])
        {
            // doubles 6s!
            await ReplyAsync($":game_die: :astonished: **{Context.User.Username}** rolls **two 6s**! Their opponent is afraid and gives up. {Context.User.Username} won **{betAmount * 3}** :coin:!");
            _ = BotDataService.AddMoney(guildId, userId, betAmount * 3);
            return;
        }

        await Task.Delay(1000);
        var opponentRolls = DiceGame.GetNextRolls(6, 2);
        await ReplyAsync($":game_die: **{Context.User.Username}**, your opponent throws their dice... and gets **{opponentRolls[0]}** and **{opponentRolls[1]}**...");
        var (total, ototal) = (rolls.Sum(c => c), opponentRolls.Sum(c => c));
        if (total > ototal)
        {
            if (rolls[0] == rolls[1])
            {
                await ReplyAsync($":game_die: **{Context.User.Username}**, you rolled a double and **won** twice your bet: {betAmount * 2} :coin:");
                _ = BotDataService.AddMoney(guildId, userId, betAmount * 2);
                return;
            }
            await ReplyAsync($":game_die: **{Context.User.Username}**, you **won** {betAmount} :coin:");
            _ = BotDataService.AddMoney(guildId, userId, betAmount);
        }
        else if (total == ototal)
        {
            await ReplyAsync($":game_die: **{Context.User.Username}**, it's a tie.");
        }
        else
        {
            await ReplyAsync($":game_die: **{Context.User.Username}**, you **lost** {betAmount} :coin:");
            _ = BotDataService.RemoveMoney(guildId, userId, betAmount);
        }
    }

}
