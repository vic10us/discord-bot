using System;
using System.IO;
using System.Threading.Tasks;
using bot.Features.Caching;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Data.MongoDB;
using v10.Services.Images;
using static bot.Modules.UserInfoModule;

namespace bot.Modules;

public class UserInfoInteractionModule : CustomInteractionModule<SocketInteractionContext>
{
    public IImageApiService ImageService { get; set; }
    public IBotDataService BotDataService { get; set; }

    public UserInfoInteractionModule(
        IServiceProvider serviceProvider,
        IImageApiService imageService,
        IBotDataService botDataService,
        ILogger<UserInfoInteractionModule> logger
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        var database = server.Multiplexer.GetDatabase();
        _logger = logger;
        ImageService = imageService;
        BotDataService = botDataService;
        _cacheContext = new CacheContext<SocketInteractionContext>(database, logger);
    }

    [SlashCommand("rank", "Get your Rank Card or the rank of another user")]
    public async Task GetRank(IGuildUser user = null)
    {
        await DeferAsync();
        user ??= Context.User as SocketGuildUser;
        var imageStreamResult = await _cacheContext.WithLock<Stream>(async () => {
            var imageStream = await GetImageStream(user);
            return imageStream;
        });
        imageStreamResult.IfSucc(async imageStream => {
            await ModifyOriginalResponseAsync((mp) => {
                imageStream.Seek(0, SeekOrigin.Begin);
                var embed = new EmbedBuilder()
                    .WithTitle($"{user.DisplayName} Rank Card")
                    .WithImageUrl($"attachment://{TemplateConstants.RankImage}")
                    .WithColor(Color.Blue)
                    .Build();
                mp.Embeds = new[] { embed };
                mp.Attachments = new[] { new FileAttachment(imageStream, TemplateConstants.RankImage) };
            });
        });
        imageStreamResult.IfFail(async error => {
            await FollowupAsync("error");
            await FollowupAsync("Error getting rank card.", ephemeral: true);
            await DeleteOriginalResponseAsync();
        });
    }

    private async Task<Stream> GetImageStream(IUser user)
    {
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
        return imageStream;
    }
}
