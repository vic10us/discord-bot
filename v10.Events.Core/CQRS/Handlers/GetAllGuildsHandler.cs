using AutoMapper;
using LanguageExt.Common;
using MediatR;
using v10.Data.MongoDB;
using v10.Events.Core.CQRS.Queries;

namespace v10.Events.Core.CQRS.Handlers;

public class GetAllGuildsHandler : IRequestHandler<GetAllGuildsQuery, Result<List<Dtos.Guild>>>
{
    private readonly IBotDataService _botDataService;
    private readonly IMapper _mapper;

    public GetAllGuildsHandler(IBotDataService botDataService, IMapper mapper)
    {
        _botDataService = botDataService;
        _mapper = mapper;
    }

    public async Task<Result<List<Dtos.Guild>>> Handle(GetAllGuildsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return _mapper.Map<List<Dtos.Guild>>(await _botDataService.GetGuildsAsync());
        }
        catch (Exception ex)
        {
            return new Result<List<Dtos.Guild>>(ex);
        }
    }
}
