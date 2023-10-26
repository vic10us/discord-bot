using MediatR;
using v10.Data.MongoDB;
using Discord.WebSocket;
using Discord;
using v10.Bot.Discord;
using v10.Events.Core.Commands;

namespace v10.Events.Core.CQRS.Handlers;

public class UserLevelChangedCommandHandler : IRequestHandler<UserLevelChangedCommand>
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IDiscordMessageService _messageService;

    public UserLevelChangedCommandHandler(
            DiscordSocketClient discordSocketClient,
            IDiscordMessageService messageService)
    {
        _discordSocketClient = discordSocketClient;
        _messageService = messageService;
    }

    public async Task Handle(UserLevelChangedCommand request, CancellationToken cancellationToken)
    {
        var user = _discordSocketClient.GetUser(request.UserId);
        if (request.Direction == "down")
        {
            await _messageService.SendMessageAsync(request.GuildId, "level.log", $"Oh no {user.Mention}! You've lost a level! Your {request.Type} level is now {request.NewLevel}!", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone), cancellationToken: cancellationToken);
            return;
        }
        await _messageService.SendMessageAsync(request.GuildId, "level.log", $"Congratulations {user.Mention}! You've leveled up! Your {request.Type} level is now {request.NewLevel}!", allowedMentions: new AllowedMentions(AllowedMentionTypes.Everyone), cancellationToken: cancellationToken);
        return;
    }
}

