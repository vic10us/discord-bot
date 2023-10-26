using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Events.Core.Commands;
using v10.Events.Core.Enums;

namespace bot.Modules;

public class EconomyInteractionModule : CustomInteractionModule<SocketInteractionContext>
{
    // private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public EconomyInteractionModule(
        ILogger<EconomyInteractionModule> logger, 
        IMediator mediator, 
        IServiceProvider serviceProvider
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        _database = server.Multiplexer.GetDatabase();
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("xp", "Add/Remove/Set Xp for a User")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task XpCommand(XpOperationType operation, ulong amount, IGuildUser user, XpType xpType = XpType.Text)
    {
        var levelData = operation switch
        {
            XpOperationType.Add => await _mediator.Send(new AddUserXpCommand() { 
                GuildId = Context.Guild.Id,
                UserId = user.Id,
                Amount = amount,
                Type = xpType
            }),
            XpOperationType.Remove => await _mediator.Send(new RemoveUserXpCommand()
            {
                GuildId = Context.Guild.Id,
                UserId = user.Id,
                Amount = amount,
                Type = xpType
            }),
            XpOperationType.Set => await _mediator.Send(new SetUserXpCommand()
            {
                GuildId = Context.Guild.Id,
                UserId = user.Id,
                Amount = amount,
                Type = xpType
            }),
            _ => null,
        };
        if (levelData == null) {
            await RespondAsync("Invalid Operation");
            return;
        }

        await RespondAsync($"User XP is now {levelData.totalXp}. Level is {levelData.level}, Voice XP is {levelData.totalVoiceXp}, Voice Level is {levelData.voiceLevel}");
    }
}
