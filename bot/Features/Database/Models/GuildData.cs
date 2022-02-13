using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bot.Features.Database.Models
{
    public class GuildData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string guildId { get; set; }
        public IDictionary<string, string> channelNotifications { get; set; } = new Dictionary<string, string>();

        public GuildData() {}

        public GuildData(string id, GuildData data)
        {
            Id = id;
            guildId = data.guildId;
            channelNotifications = data.channelNotifications;
        }

    }
}