using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace v10.Data.Abstractions.Models;

public class Guild
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string guildId { get; set; }
    public IDictionary<string, string> channelNotifications { get; set; } = new Dictionary<string, string>();
    public ulong[] staffRoles { get; set; } = Array.Empty<ulong>();

    public Guild() { }

    public Guild(string id, Guild guild)
    {
        Id = id;
        guildId = guild.guildId;
        channelNotifications = guild.channelNotifications;
        staffRoles = guild.staffRoles;
    }
}
