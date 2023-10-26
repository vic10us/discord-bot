using MediatR;
using v10.Data.MongoDB;
using Discord.WebSocket;
using Discord;
using v10.Data.Abstractions.Models;
using v10.Bot.Discord;
using v10.Events.Core.Commands;
using v10.Events.Core.Enums;

namespace v10.Events.Core.CQRS.Handlers;

public class RemoveUserXpCommandHandler : IRequestHandler<RemoveUserXpCommand, LevelData>
{
    private readonly IBotDataService _botDataService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IMediator _mediator;
    private readonly IDiscordMessageService _messageService;

    public RemoveUserXpCommandHandler(
            IBotDataService botDataService,
            DiscordSocketClient discordSocketClient,
            IMediator mediator,
            IDiscordMessageService messageService)
    {
        _botDataService = botDataService;
        _discordSocketClient = discordSocketClient;
        _mediator = mediator;
        _messageService = messageService;
    }

    public async Task<LevelData> Handle(RemoveUserXpCommand request, CancellationToken cancellationToken)
    {
        var user = _discordSocketClient.GetUser(request.UserId);

        Action<ulong> callback = (newLevel) =>
        {
            _mediator.Send(new UserLevelChangedCommand()
            {
                GuildId = request.GuildId,
                UserId = request.UserId,
                NewLevel = newLevel,
                Direction = "down",
                Type = request.Type
            });
        };

        var newData = request.Type switch
        {
            XpType.Text => _botDataService.RemoveXp(request.GuildId, request.UserId, request.Amount, callback),
            XpType.Voice => _botDataService.RemoveVoiceXp(request.GuildId, request.UserId, request.Amount, callback),
            _ => null
        };

        if (newData == null) return null;
        var newXp = request.Type switch
        {
            XpType.Text => newData.totalXp,
            XpType.Voice => newData.totalVoiceXp,
            _ => default
        };
        await _messageService.SendMessageAsync(request.GuildId, "system.log", $"Removed {request.Amount} {request.Type} xp from User {user.Mention}! New {request.Type} XP is {newXp}", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone), cancellationToken: cancellationToken);
        return newData;
    }
}

