using AutoMapper;
using bot.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.MongoDB;

namespace bot.Handlers;

public class GetGuildByIdHandler : IRequestHandler<GetGuildByIdQuery, Dtos.Guild>
{
  private readonly BotDataService _botDataService;
  private readonly IMapper _mapper;

  public GetGuildByIdHandler(BotDataService botDataService, IMapper mapper)
  {
    _botDataService = botDataService;
    _mapper = mapper;
  }

  public Task<Dtos.Guild> Handle(GetGuildByIdQuery request, CancellationToken cancellationToken)
  {
    var guild = _botDataService.GetGuild(request.GuildId, canCreate: false);
    var result = _mapper.Map<Dtos.Guild>(guild);
    return Task.FromResult(result);
  }
}
