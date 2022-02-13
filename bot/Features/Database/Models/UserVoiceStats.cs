using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bot.Features.Database.Models;

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
