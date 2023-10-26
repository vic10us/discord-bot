using LanguageExt.Common;
using MediatR;

namespace v10.Events.Core.CQRS.Queries;

public class GetGuildByIdQuery : IRequest<Result<Dtos.Guild>>
{
    public ulong GuildId { get; }

    public GetGuildByIdQuery(ulong guildId)
    {
        GuildId = guildId;
    }
}
