using MediatR;
using System.Collections.Generic;

namespace bot.Commands;

public class UpdateGuildCommand : IRequest<ulong>
{
    public ulong GuildId { get; set; }
    public IDictionary<string, string> ChannelNotifications { get; set; } = new Dictionary<string, string>();
}
