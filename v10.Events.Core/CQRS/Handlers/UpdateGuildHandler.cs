using MediatR;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;
using v10.Events.Core.Commands;

namespace v10.Events.Core.CQRS.Handlers;

public class UpdateGuildHandler : IRequestHandler<UpdateGuildCommand, ulong>
{
    private readonly IBotDataService _botDataService;

    public UpdateGuildHandler(IBotDataService botDataService)
    {
        _botDataService = botDataService;
    }

    public Task<ulong> Handle(UpdateGuildCommand request, CancellationToken cancellationToken)
    {
        var x = new Guild
        {
            guildId = $"{request.GuildId}",
            guildName = $"{request.GuildName}",
            channelNotifications = request.ChannelNotifications,
            staffRoles = request.StaffRoles
        };
        _botDataService.UpdateGuild(StringToUInt64(request.GuildId), x);
        return Task.FromResult(StringToUInt64(request.GuildId));
    }

    ulong StringToUInt64(string value)
        => ulong.TryParse(value, out var val) ? val : default;

}
