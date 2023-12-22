namespace v10.Events.Core.Dtos;

public class CreateGuildRequest
{
    public string GuildId { get; set; }
    public string GuildName { get; set; }
    public IDictionary<string, string> ChannelNotifications { get; set; } = new Dictionary<string, string>();
    public string[] StaffRoles { get; set; } = Array.Empty<string>();
}
