using MediatR;
using System;
using System.Collections.Generic;

namespace bot.Commands;

public class CreateGuildCommand : IRequest<Dtos.Guild>
{
    public string GuildId { get; set; }
    public string GuildName { get; set; }
    public IDictionary<string, string> ChannelNotifications { get; set; } = new Dictionary<string, string>();
    public string[] StaffRoles { get; set; } = Array.Empty<string>();
}
