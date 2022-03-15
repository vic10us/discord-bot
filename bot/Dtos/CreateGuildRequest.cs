using System.Collections.Generic;

namespace bot.Dtos;

public class CreateGuildRequest
{
  public ulong GuildId { get; set; }
  public IDictionary<string, string> ChannelNotifications { get; set; } = new Dictionary<string, string>();
}
