using AutoMapper;
using bot.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;

namespace bot.Handlers;

public class CreateGuildHandler : IRequestHandler<CreateGuildCommand, Dtos.Guild>
{
    private readonly BotDataService _botDataService;
    private readonly IMapper _mapper;

    public CreateGuildHandler(BotDataService botDataService, IMapper mapper)
    {
        _botDataService = botDataService;
        _mapper = mapper;
    }

    public async Task<Dtos.Guild> Handle(CreateGuildCommand request, CancellationToken cancellationToken)
    {
        var x = new Guild
        {
            guildId = $"{request.GuildId}",
            channelNotifications = request.ChannelNotifications,
            staffRoles = request.StaffRoles
        };
        return _mapper.Map<Dtos.Guild>(await _botDataService.CreateGuildAsync(x));
    }
}
