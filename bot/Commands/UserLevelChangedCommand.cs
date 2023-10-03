using bot.Modules.Enums;
using MediatR;

namespace bot.Commands;

public class UserLevelChangedCommand : IRequest
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public XpType Type { get; set; }
    public ulong NewLevel { get; set; }
    public string Direction { get; set; } = "up";
}
