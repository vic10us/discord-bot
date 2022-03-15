using MediatR;

namespace bot.Queries;

public class GetGuildByIdQuery : IRequest<Dtos.Guild>
{
  public ulong GuildId { get; }

  public GetGuildByIdQuery(ulong guildId)
  {
    GuildId = guildId;
  }
}
