using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using bot.Features.Caching;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Data.MongoDB;
using v10.Services.Images;

namespace bot.Modules;

public class UserInfoModule : CustomModule<SocketCommandContext>
{
    public IImageApiService ImageService { get; set; }
    public IBotDataService BotDataService { get; set; }

    public UserInfoModule(
        IServiceProvider serviceProvider,
        IImageApiService imageService,
        IBotDataService botDataService,
        ILogger<UserInfoModule> logger
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        var database = server.Multiplexer.GetDatabase();
        _logger = logger;
        ImageService = imageService;
        BotDataService = botDataService;
        _cacheContext = new CacheContext<SocketCommandContext>(database, logger);
    }

    public static class TemplateConstants
    {
        public const string Rank = "rank.hbs";
        public const string RankImage = "rank.png";
        public const string OnePixelImage = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
    }

    [Command("rank")]
    [Alias("r")]
    public async Task GetNewRank(IUser user = null)
    {
        await _cacheContext.WithLock(async () => {
            user ??= Context.User;
            var guildId = Context.Guild?.Id ?? 0;
            var userId = user.Id;
            var userData = BotDataService.GetLevelData(guildId, userId);
            var data = new RankCardRequest();

            var gaid = (user as SocketGuildUser)?.GuildAvatarId;
            var url = (string.IsNullOrWhiteSpace(gaid)) ? user.GetAvatarUrl(ImageFormat.Png)
                : $"https://cdn.discordapp.com/guilds/{guildId}/users/{userId}/avatars/{gaid}.png";
            var guildUserAvatarUrl = $"https://cdn.discordapp.com/guilds/{guildId}/users/{userId}/avatars/{gaid}.png";
            var userAvatarUrl = user.GetAvatarUrl(ImageFormat.Png);
            var guser = user as SocketGuildUser;

            data.rank = (int)BotDataService.GetUserRank(guildId, userId);
            data.userName = $"{guser.DisplayName}";
            data.cardTitle = " ";
            data.userDescriminator = user.Discriminator;
            var (textLevel, textXp, xpForNextTextLevel) = BotDataService.ComputeLevelAndXp(userData.level, userData.xp);
            var (voiceLevel, voiceXp, xpForNextVoiceLevel) = BotDataService.ComputeLevelAndXp(userData.voiceLevel, userData.voiceXp);
            data.textLevel = (int)textLevel;
            data.textXp = (int)textXp;
            data.xpForNextTextLevel = (int)xpForNextTextLevel;
            data.voiceLevel = (int)voiceLevel;
            data.voiceXp = (int)voiceXp;
            data.xpForNextVoiceLevel = (int)xpForNextVoiceLevel;
            data.avatarUrl = url;

            var imageStream = await ImageService.CreateRankCard(data);
            await SendImageEmbed(imageStream, $"{user.Username} Rank Card", TemplateConstants.RankImage, Color.Blue);
        });
    }

    [Command("userinfo")]
    public async Task UserInfoAsync(IUser user = null)
    {
        await _cacheContext.WithLock(async () => {
            user ??= Context.User;
            var message = $"{user} is {user.Id} [{user.Status}] {user.GetAvatarUrl()}";
            await ReplyAsync(message);
        });
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
        await _cacheContext.WithLock(async () => {
            //user.Guild.AddGuildUserAsync()
            await user.Guild.AddBanAsync(user, reason: reason);
            await ReplyAsync("ok!");
        });
    }

    [Command("addxp")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task AddXp(IUser user, ulong amount)
    {
        await _cacheContext.WithLock(() =>
        {
            var guildId = Context.Guild?.Id ?? 0;
            var userId = user.Id;
            _ = BotDataService.AddXp(guildId, userId, amount);
            return Task.CompletedTask;
        });
    }

    [Command("removexp")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task RemoveXp(IUser user, ulong amount)
    {
        await _cacheContext.WithLock(() =>
        {
            var guildId = Context.Guild?.Id ?? 0;
            var userId = user.Id;
            _ = BotDataService.RemoveXp(guildId, userId, amount);
            return Task.CompletedTask;
        });
    }

    private static byte[] CopyToArray(Stream stream)
    {
        using var result = new MemoryStream();
        stream.CopyTo(result);
        return result.ToArray();
    }

    private static async Task<(string, Stream)> GetStreamAsync(string url)
    {
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        // add Content, Headers, etc to request
        // request.Content = new StringContent(yourJsonString, System.Text.Encoding.UTF8, "application/json");
        // request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var contentType = response.Content.Headers.ContentType.MediaType;
        return (contentType, stream);
    }

    private static async Task<(string, byte[])> DownloadImage(string url)
    {
        var (ct, str) = await GetStreamAsync(url);
        //using var client = new HttpClient();
        //var image = await client.GetStreamAsync(new Uri(url));
        return (ct, CopyToArray(str));
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
}
