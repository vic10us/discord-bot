using bot.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.MongoDB;
using Discord.WebSocket;
using Discord;
using v10.Data.Abstractions.Models;
using bot.Modules;
using System;

namespace bot.Handlers;

public class SetUserXpCommandHandler : IRequestHandler<SetUserXpCommand, LevelData>
{
    private readonly BotDataService _botDataService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IMediator _mediator;

    public SetUserXpCommandHandler(
            BotDataService botDataService,
            DiscordSocketClient discordSocketClient,
            IMediator mediator)
    {
        _botDataService = botDataService;
        _discordSocketClient = discordSocketClient;
        _mediator = mediator;
    }

    public async Task<LevelData> Handle(SetUserXpCommand request, CancellationToken cancellationToken)
    {
        var user = _discordSocketClient.GetUser(request.UserId);

        Action<ulong, string> callback = (newLevel, direction) =>
        {
            _mediator.Send(new UserLevelChangedCommand()
            {
                GuildId = request.GuildId,
                UserId = request.UserId,
                NewLevel = newLevel,
                Direction = direction,
                Type = request.Type
            });
        };

        var newData = request.Type switch
        {
            XpType.Text => _botDataService.SetXp(request.GuildId, request.UserId, request.Amount, callback),
            XpType.Voice => _botDataService.SetVoiceXp(request.GuildId, request.UserId, request.Amount, callback),
            _ => null
        };

        if (newData == null) return null;

        await SendMessageAsync(request.GuildId, "system.log", $"Set the {request.Type} XP for User {user.Mention} to {request.Amount}", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone), cancellationToken: cancellationToken);
        return newData;
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

