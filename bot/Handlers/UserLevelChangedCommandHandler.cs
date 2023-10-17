using bot.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.MongoDB;
using Discord.WebSocket;
using Discord;
using v10.Bot.Discord;

namespace bot.Handlers;

public class UserLevelChangedCommandHandler : IRequestHandler<UserLevelChangedCommand>
{
    private readonly BotDataService _botDataService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IDiscordMessageService _messageService;

    public UserLevelChangedCommandHandler(
            BotDataService botDataService,
            DiscordSocketClient discordSocketClient,
            IDiscordMessageService messageService)
    {
        _botDataService = botDataService;
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

