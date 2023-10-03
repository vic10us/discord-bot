using MediatR;

namespace bot.Commands;

public class UserVoiceLevelUp : IRequest
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public int NewLevel { get; set; }
}
