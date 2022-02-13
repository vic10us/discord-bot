using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using bot.Features.DadJokes;
using bot.Features.Database;
using bot.Features.Database.Models;
using bot.Features.Games;
using bot.Features.MondayQuotes;
using bot.Features.Pictures;
using bot.Features.RedneckJokes;
using bot.Services;
using Discord;
using Discord.Commands;
using HandlebarsDotNet;
using ContextType = Discord.Commands.ContextType;

namespace bot;

// Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
public class InfoModule : ModuleBase<SocketCommandContext>
{
    public DiceGame DiceGame { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public PictureService PictureService { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public DadJokeService DadJokeService { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public MondayQuotesService MondayQuotesService { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public RedneckJokeService RedneckJokeService { get; set; }
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public ImageService ImageService { get; set; }
    public BotDataService BotDataService { get; set; }

    [Command("ping")]
    [Alias("pong", "hello")]
    public Task PingAsync()
        => ReplyAsync("pong!");

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

    private async Task SendImageEmbed(Stream fileStream, string title, string filename, Color color)
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithImageUrl($"attachment://{filename}")
            .WithColor(color)
            .Build();
        await Context.Channel.SendFileAsync(stream: fileStream, filename: filename, embed: embed);
    }

    [Command("bunny")]
    public async Task GetBunny()
    {
        var (filename, stream) = PictureService.GetPictureFromCategory("bunny");
        stream.Seek(0, SeekOrigin.Begin);
        var ext = Path.GetExtension(filename);
        await SendImageEmbed(stream, "Random Bunny", $"bunny.{ext}", Color.Green);
    }

    [Command("seacreature")]
    [Alias("sc", "creature")]
    public async Task GetSeaCreature()
    {
        var (filename, stream) = PictureService.GetPictureFromCategory("seacreature");
        stream.Seek(0, SeekOrigin.Begin);
        var ext = Path.GetExtension(filename);
        await SendImageEmbed(stream, "Random Sea Creature", $"seacreature.{ext}", Color.Green);
    }

    private static async Task<string> GetHandleImageTemplate(string name)
    {
        var assembly = Assembly.GetEntryAssembly();
        var resourceStream = assembly?.GetManifestResourceStream($"bot.Features.Images.Templates.{name}");
        if (resourceStream == null) return "";
        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private static byte[] CopyToArray(Stream stream)
    {
        using var result = new MemoryStream();
        stream.CopyTo(result);
        return result.ToArray();
    }

    private static async Task<byte[]> DownloadImage(string url)
    {
        using var client = new HttpClient();
        var image = await client.GetStreamAsync(new Uri(url));
        return CopyToArray(image);
    }

    private static string GetStatusColor(UserStatus status)
    {
        return status switch
        {
            UserStatus.Idle => "#FAA51B",
            UserStatus.Invisible => "#747F8D",
            UserStatus.Online => "#44B37F",
            UserStatus.AFK => "#FAA51B",
            UserStatus.DoNotDisturb => "#F04848",
            UserStatus.Offline => "#747F8D",
            _ => "#747F8D"
        };
    }

    [NamedArgumentType]
    public class NamableArguments
    {
        public string First { get; set; }
        public string Second { get; set; }
        public string Third { get; set; }
        public string Fourth { get; set; }
    }

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
        var d = BotDataService.AddMoney(guildId, userId, (ulong)amount);
        await ReplyAsync($"Added **{amount}** :coin: to **{user.Username}**. Their new balance is **{d.money}** :coin:");
    }

    [Command("removecoins")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task RemoveMoney(IUser user, ulong amount)
    {
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var d = BotDataService.RemoveMoney(guildId, userId, (ulong)amount);
        await ReplyAsync($"Removed **{amount}** :coin: from **{user.Username}**. Their new balance is: **{d.money}** :coin:");
    }

    [Command("addxp")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task AddXp(IUser user, ulong amount)
    {
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        _ = BotDataService.AddXp(guildId, userId, (ulong)amount);
        return Task.CompletedTask;
    }

    [Command("removexp")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task RemoveXp(IUser user, ulong amount)
    {
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        _ = BotDataService.RemoveXp(guildId, userId, (ulong)amount);
        return Task.CompletedTask;
    }

    [Command("rank")]
    [Alias("r")]
    public async Task GetRank(IUser user = null)
    {
        user ??= Context.User;
        var guildId = Context.Guild?.Id ?? 0;
        var userId = user.Id;
        var userData = BotDataService.GetLevelData(guildId, userId);

        var source = await GetHandleImageTemplate("rank.hbs");
        var template = Handlebars.Compile(source);
        var data = new RankData();

        var profileImageData = await DownloadImage(user.GetAvatarUrl(ImageFormat.Png));

        data.avatar.image = $"data:image/png;base64,{Convert.ToBase64String(profileImageData)}";
        data.name = user.Username;
        data.code = user.Discriminator;
        data.rank = BotDataService.GetUserRank(guildId, userId);
        (data.level, data.xp.current, data.xp.required) = BotDataService.ComputeLevelAndXp(userData.level, userData.xp);

        // data.xp.current = 500;
        data.statusColor = GetStatusColor(user.Status);

        var svg = template(data);

        var imageStream = await ImageService.ConvertSvgImage(svg);
        await SendImageEmbed(imageStream, $"{user.Username}#{user.Discriminator} Rank Card", "rank.png", Color.Blue);
    }

    [Command("qrcode")]
    [Alias("qr")]
    public async Task GetQRCode(IUser user = null)
    {
        user ??= Context.User;
        var imageStream = await ImageService.CreateQRCode();
        // var image = System.Drawing.Image.FromStream(imageStream);
        await SendImageEmbed(imageStream, $"{user.Username}#{user.Discriminator} QR Code", "qrcode.png", Color.Blue);
    }

    [Command("bunnycat")]
    [Alias("bc")]
    public async Task GetBunnyCat()
    {
        var stream = PictureService.GetPictureFromCategory("bunnycat");
        stream.Item2.Seek(0, SeekOrigin.Begin);
        var ext = Path.GetExtension(stream.Item1);
        await SendImageEmbed(stream.Item2, "Random Bunny with Cat", $"bunnycat.{ext}", Color.Green);
    }

    [Command("cat")]
    public async Task CatAsync()
    {
        // Get a stream containing an image of a cat
        var stream = await PictureService.GetCatPictureAsync();
        await SendImageEmbed(stream, "Random Cat", $"cat.png", Color.Green);
    }

    [Command("dadjoke")]
    [Alias("dj")]
    public async Task DadJoke()
    {
        var joke = await DadJokeService.GetDadJoke();
        await ReplyAsync(joke.Joke, messageReference: new MessageReference(Context.Message.Id));
    }

    [Command("monday")]
    public async Task MondayQuote()
    {
        var joke = await MondayQuotesService.GetQuote();
        await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
    }

    [Command("redneckjoke")]
    [Alias("redneck", "rn")]
    public async Task RedneckJoke()
    {
        var joke = await RedneckJokeService.GetQuote();
        await ReplyAsync(joke, messageReference: new MessageReference(Context.Message.Id));
    }

    [Command("userinfo")]
    public async Task UserInfoAsync(IUser user = null)
    {
        user ??= Context.User;
        var message = $"{user} is {user.Id} [{user.Status}] {user.GetAvatarUrl()}";
        await ReplyAsync(message);
    }

    // Ban a user
    [Command("ban")]
    [RequireContext(ContextType.Guild)]
    // make sure the user invoking the command can ban
    [RequireUserPermission(GuildPermission.BanMembers)]
    // make sure the bot itself can ban
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
    {
        //user.Guild.AddGuildUserAsync()
        await user.Guild.AddBanAsync(user, reason: reason);
        await ReplyAsync("ok!");
    }

    // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
    [Command("echo")]
    public Task EchoAsync([Remainder] string text)
        // Insert a ZWSP before the text to prevent triggering other bots!
        => ReplyAsync('\u200B' + text, messageReference: new MessageReference(Context.Message.Id));

    // 'params' will parse space-separated elements into a list
    [Command("list")]
    public Task ListAsync(params string[] objects)
        => ReplyAsync("You listed: " + string.Join("; ", objects));

    // Setting a custom ErrorMessage property will help clarify the precondition error
    [Command("guild_only")]
    [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
    public Task GuildOnlyCommand()
        => ReplyAsync("Nothing to see here!");

    // ~say hello world -> hello world
    [Command("say")]
    [Summary("Echoes a message.")]
    public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        => ReplyAsync(echo);
}
