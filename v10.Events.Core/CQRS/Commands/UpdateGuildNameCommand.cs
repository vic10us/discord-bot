using MediatR;

namespace v10.Events.Core.Commands;

public class UpdateGuildNameCommand : IRequest
{
    public string GuildId { get; set; }
    public string GuildName { get; set; }
}
