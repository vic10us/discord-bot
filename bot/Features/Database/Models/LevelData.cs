using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bot.Features.Database.Models;

public class LevelData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string userId { get; set; }
    public string guildId { get; set; }
    public ulong xp { get; set; } = 0;
    public ulong level { get; set; } = 0;
    public ulong totalXp { get; set; } = 0;
    public ulong messageCount { get; set; } = 0;
    public ulong money { get; set; } = 0;
    public DateTimeOffset lastUpdated { get; set; } = DateTimeOffset.UtcNow;

    public LevelData() { }

    public LevelData(string guildId, string userId)
    {
        this.guildId = guildId;
        this.userId = userId;
    }
}

public class MessageThrottle
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string userId { get; set; }
    public string guildId { get; set; }
    public string key { get; set; }
    public DateTimeOffset lastUpdated { get; set; } = DateTimeOffset.UtcNow;
    [BsonElement("expiry")]
    public BsonDateTime expiry { get; set; }

    public MessageThrottle(string guildId, string userId, string key)
    {
        this.guildId = guildId;
        this.userId = userId;
        this.key = key;
    }
}
