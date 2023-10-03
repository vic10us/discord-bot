using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace v10.Data.Abstractions.Models;

public class GuildAutoRoles
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string guildId { get; set; }
    public string category { get; set; }
}
