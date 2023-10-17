using MediatR;

namespace bot.Commands;

public class UpdateGuildNameCommand : IRequest
{
    public string GuildId { get; set; }
    public string GuildName { get; set; }
}
