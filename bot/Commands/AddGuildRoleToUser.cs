using MediatR;

namespace bot.Commands;

public class AddGuildRoleToUser : IRequest<bool>
{
    public ulong GuildId { get; set; }
    public ulong RoleId { get; set; }
    public ulong UserId { get; set; }
}
