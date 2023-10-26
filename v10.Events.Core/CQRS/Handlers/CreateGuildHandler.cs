using AutoMapper;
using MediatR;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;
using v10.Events.Core.Commands;

namespace v10.Events.Core.CQRS.Handlers;

public class CreateGuildHandler : IRequestHandler<CreateGuildCommand, Dtos.Guild>
{
    private readonly IBotDataService _botDataService;
    private readonly IMapper _mapper;

    public CreateGuildHandler(IBotDataService botDataService, IMapper mapper)
    {
        _botDataService = botDataService;
        _mapper = mapper;
    }

    public async Task<Dtos.Guild> Handle(CreateGuildCommand request, CancellationToken cancellationToken)
    {
        var x = new Guild
        {
            guildId = $"{request.GuildId}",
            guildName = $"{request.GuildName}",
            channelNotifications = request.ChannelNotifications,
            staffRoles = request.StaffRoles
        };
        return _mapper.Map<Dtos.Guild>(await _botDataService.CreateGuildAsync(x));
    }
}
