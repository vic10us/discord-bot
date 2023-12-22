using LanguageExt.Common;
using MediatR;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;
using v10.Events.Core.Commands;

namespace v10.Events.Core.CQRS.Handlers;

public class UpdateGuildHandler : IRequestHandler<UpdateGuildCommand, Result<ulong>>
{
    private readonly IBotDataService _botDataService;

    public UpdateGuildHandler(IBotDataService botDataService)
    {
        _botDataService = botDataService;
    }

    public Task<Result<ulong>> Handle(UpdateGuildCommand request, CancellationToken cancellationToken)
    {
        var x = new Guild
        {
            guildId = $"{request.GuildId}",
            guildName = $"{request.GuildName}",
            channelNotifications = request.ChannelNotifications,
            staffRoles = request.StaffRoles
        };

        var guildId = StringToUInt64(request.GuildId);
        try
        {
            _botDataService.UpdateGuild(guildId, x);
            return Task.FromResult(new Result<ulong>(guildId));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new Result<ulong>(ex));
        }
    }

    ulong StringToUInt64(string value)
        => ulong.TryParse(value, out var val) ? val : default;

}
