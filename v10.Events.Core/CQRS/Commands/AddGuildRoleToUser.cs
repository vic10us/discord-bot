using LanguageExt.Common;
using MediatR;

namespace v10.Events.Core.Commands;

public class AddGuildRoleToUser : IRequest<Result<bool>>
{
    public ulong GuildId { get; set; }
    public ulong RoleId { get; set; }
    public ulong UserId { get; set; }
}
