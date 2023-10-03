using bot.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;

namespace bot.Handlers;

public class UpdateGuildHandler : IRequestHandler<UpdateGuildCommand, ulong>
{
    private readonly BotDataService _botDataService;

    public UpdateGuildHandler(BotDataService botDataService)
    {
        _botDataService = botDataService;
    }

    public Task<ulong> Handle(UpdateGuildCommand request, CancellationToken cancellationToken)
    {
        var x = new Guild
        {
            guildId = $"{request.GuildId}",
            channelNotifications = request.ChannelNotifications,
            staffRoles = request.StaffRoles
        };
        _botDataService.UpdateGuild(StringToUInt64(request.GuildId), x);
        return Task.FromResult(StringToUInt64(request.GuildId));
    }

    ulong StringToUInt64(string value)
        => ulong.TryParse(value, out ulong val) ? val : default;

}
