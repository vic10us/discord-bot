﻿using bot.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.MongoDB;
using Discord.WebSocket;
using Discord;
using v10.Data.Abstractions.Models;
using System;
using bot.Modules.Enums;
using v10.Bot.Discord;

namespace bot.Handlers;

public class RemoveUserXpCommandHandler : IRequestHandler<RemoveUserXpCommand, LevelData>
{
    private readonly BotDataService _botDataService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IMediator _mediator;
    private readonly IDiscordMessageService _messageService;

    public RemoveUserXpCommandHandler(
            BotDataService botDataService,
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

