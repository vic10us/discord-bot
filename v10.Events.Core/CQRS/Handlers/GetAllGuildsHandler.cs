using AutoMapper;
using MediatR;
using v10.Data.MongoDB;
using v10.Events.Core.CQRS.Queries;

namespace v10.Events.Core.CQRS.Handlers;

public class GetAllGuildsHandler : IRequestHandler<GetAllGuildsQuery, List<Dtos.Guild>>
{
    private readonly IBotDataService _botDataService;
    private readonly IMapper _mapper;

    public GetAllGuildsHandler(IBotDataService botDataService, IMapper mapper)
    {
        _botDataService = botDataService;
        _mapper = mapper;
    }

    public async Task<List<Dtos.Guild>> Handle(GetAllGuildsQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<List<Dtos.Guild>>(await _botDataService.GetGuildsAsync());
    }
}
