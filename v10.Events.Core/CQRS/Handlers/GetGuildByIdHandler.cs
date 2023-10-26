using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;
using v10.Events.Core.CQRS.Queries;

namespace v10.Events.Core.CQRS.Handlers;

public class GetGuildByIdHandler : IRequestHandler<GetGuildByIdQuery, Dtos.Guild>
{
    private readonly IBotDataService _botDataService;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public GetGuildByIdHandler(IBotDataService botDataService, IMapper mapper, IDistributedCache cache)
    {
        _botDataService = botDataService;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<Dtos.Guild> Handle(GetGuildByIdQuery request, CancellationToken cancellationToken)
    {
        var cachedGuild = await _cache.GetStringAsync(request.GuildId.ToString(), cancellationToken);
        Guild guild;
        if (!string.IsNullOrEmpty(cachedGuild))
        {
            guild = JsonConvert.DeserializeObject<Guild>(cachedGuild);
        }
        else
        {
            guild = _botDataService.GetGuild(request.GuildId, canCreate: false);
            _cache.SetString(
                request.GuildId.ToString(),
                JsonConvert.SerializeObject(guild),
                new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10),
                });
        }
        var result = _mapper.Map<Dtos.Guild>(guild);
        return result;
    }
}
