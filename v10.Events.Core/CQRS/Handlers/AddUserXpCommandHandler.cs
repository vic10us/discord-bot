using MediatR;
using v10.Data.MongoDB;
using Discord.WebSocket;
using Discord;
using v10.Data.Abstractions.Models;
using v10.Bot.Discord;
using v10.Events.Core.Commands;
using v10.Events.Core.Enums;
using LanguageExt.Common;

namespace v10.Events.Core.CQRS.Handlers;

public class AddUserXpCommandHandler : IRequestHandler<AddUserXpCommand, Result<LevelData?>>
{
    private readonly IBotDataService _botDataService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IMediator _mediator;
    private readonly IDiscordMessageService _messageService;

    public AddUserXpCommandHandler(
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

    public async Task<Result<LevelData?>> Handle(AddUserXpCommand request, CancellationToken cancellationToken)
    {
        try { return await AddUserXp(request, cancellationToken); }
        catch (Exception ex) { return new Result<LevelData?>(ex); }
    }

    private async Task<LevelData?> AddUserXp(AddUserXpCommand request, CancellationToken cancellationToken)
    {
        var user = _discordSocketClient.GetUser(request.UserId);

        void callback(ulong newLevel)
        {
            _mediator.Send(new UserLevelChangedCommand()
            {
                GuildId = request.GuildId,
                UserId = request.UserId,
                NewLevel = newLevel,
                Direction = "up",
                Type = request.Type
            }, cancellationToken);
        }

        var newData = request.Type switch
        {
            XpType.Text => _botDataService.AddXp(request.GuildId, request.UserId, request.Amount, callback),
            XpType.Voice => _botDataService.AddVoiceXp(request.GuildId, request.UserId, request.Amount, callback),
            _ => null
        };

        if (newData == null) return null;
        var newXp = request.Type switch
        {
            XpType.Text => newData.totalXp,
            XpType.Voice => newData.totalVoiceXp,
            _ => default
        };
        await _messageService.SendMessageAsync(request.GuildId, "system.log", $"Added {request.Amount} {request.Type} XP to User {user.Mention}! New {request.Type} XP is {newXp}", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone), cancellationToken: cancellationToken);
        return newData;
    }
}

