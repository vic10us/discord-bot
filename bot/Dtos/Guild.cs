using System.Collections.Generic;

namespace bot.Dtos;

public class Guild
{
    public long GuildId { get; set; }
    public IDictionary<string, string> ChannelNotifications { get; set; } = new Dictionary<string, string>();
}
