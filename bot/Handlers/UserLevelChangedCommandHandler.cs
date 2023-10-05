using bot.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.MongoDB;
using Discord.WebSocket;
using Discord;

namespace bot.Handlers;

public class UserLevelChangedCommandHandler : IRequestHandler<UserLevelChangedCommand>
{
    private readonly BotDataService _botDataService;
    private readonly DiscordSocketClient _discordSocketClient;

    public UserLevelChangedCommandHandler(
            BotDataService botDataService, 
            DiscordSocketClient discordSocketClient
        )
    {
        _botDataService = botDataService;
        _discordSocketClient = discordSocketClient;
    }

    public async Task Handle(UserLevelChangedCommand request, CancellationToken cancellationToken)
    {
        var user = _discordSocketClient.GetUser(request.UserId);
        if (request.Direction == "down")
        {
            await SendMessageAsync(request.GuildId, "level.log", $"Oh no {user.Mention}! You've lost a level! Your {request.Type} level is now {request.NewLevel}!", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone), cancellationToken: cancellationToken);
            return;
        }
        await SendMessageAsync(request.GuildId, "level.log", $"Congratulations {user.Mention}! You've leveled up! Your {request.Type} level is now {request.NewLevel}!", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone), cancellationToken: cancellationToken);
        return;
    }

    private async Task SendMessageAsync(ulong guildId, string route, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, CancellationToken cancellationToken = default)
    {
        var guildData = _botDataService.GetGuild(guildId);
        if (guildData == null) return;
        if (!guildData.channelNotifications.ContainsKey(route)) return;
        var channelId_str = guildData.channelNotifications[route];
        if (string.IsNullOrWhiteSpace(channelId_str)) return;
        if (!ulong.TryParse(channelId_str, out var channelId)) return;
        await SendMessageAsync(channelId, message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags, cancellationToken);
    }

    private async Task SendMessageAsync(ulong channelId, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, CancellationToken cancellationToken = default)
    {
        var channel = _discordSocketClient.GetChannel(channelId);
        await (channel as IMessageChannel)?.SendMessageAsync(message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
    }
}

