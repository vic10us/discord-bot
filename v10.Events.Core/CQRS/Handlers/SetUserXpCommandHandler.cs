﻿using MediatR;
using v10.Data.MongoDB;
using Discord.WebSocket;
using Discord;
using v10.Data.Abstractions.Models;
using v10.Bot.Discord;
using v10.Events.Core.Commands;
using v10.Events.Core.Enums;
using LanguageExt.Common;

namespace v10.Events.Core.CQRS.Handlers;

public class SetUserXpCommandHandler : IRequestHandler<SetUserXpCommand, Result<LevelData?>>
{
    private readonly IBotDataService _botDataService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IMediator _mediator;
    private readonly IDiscordMessageService _messageService;

    public SetUserXpCommandHandler(
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

    public async Task<Result<LevelData?>> Handle(SetUserXpCommand request, CancellationToken cancellationToken)
    {
        try { return await SetUserXp(request, cancellationToken); }
        catch (Exception ex) { return new Result<LevelData?>(ex); }
    }

    private async Task<LevelData?> SetUserXp(SetUserXpCommand request, CancellationToken cancellationToken)
    {
        var user = _discordSocketClient.GetUser(request.UserId);

        void callback(ulong newLevel, string direction)
        {
            _mediator.Send(new UserLevelChangedCommand()
            {
                GuildId = request.GuildId,
                UserId = request.UserId,
                NewLevel = newLevel,
                Direction = direction,
                Type = request.Type
            }, cancellationToken);
        }

        var newData = request.Type switch
        {
            XpType.Text => _botDataService.SetXp(request.GuildId, request.UserId, request.Amount, callback),
            XpType.Voice => _botDataService.SetVoiceXp(request.GuildId, request.UserId, request.Amount, callback),
            _ => null
        };

        if (newData == null) return null;

        await _messageService.SendMessageAsync(request.GuildId, "system.log", $"Set the {request.Type} XP for User {user.Mention} to {request.Amount}", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone), cancellationToken: cancellationToken);
        return newData;
    }
}

