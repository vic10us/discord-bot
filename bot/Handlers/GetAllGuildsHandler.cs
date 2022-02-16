using AutoMapper;
using bot.Features.Database;
using bot.Queries;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Handlers;

public class GetAllGuildsHandler : IRequestHandler<GetAllGuildsQuery, List<Dtos.Guild>>
{
    private readonly BotDataService _botDataService;
    private readonly IMapper _mapper;

    public GetAllGuildsHandler(BotDataService botDataService, IMapper mapper)
    {
        _botDataService = botDataService;
        _mapper = mapper;
    }

    public async Task<List<Dtos.Guild>> Handle(GetAllGuildsQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<List<Dtos.Guild>>(await _botDataService.GetGuildsAsync());
    }
}
