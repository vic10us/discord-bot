using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace v10.Data.Abstractions.Models;

public class UserVoiceStats
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string userId { get; set; }
    public string guildId { get; set; }
    public string channelId { get; set; }
    public bool isActive { get; set; }
    public DateTimeOffset lastJoinedAt { get; set; }
    public DateTimeOffset lastExitedAt { get; set; }
    public ulong totalTimeSpentInVoice { get; set; }
}
