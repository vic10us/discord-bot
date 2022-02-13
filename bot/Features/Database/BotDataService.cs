using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bot.Configuration.Models;
using bot.Features.Database.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace bot.Features.Database
{
    public static class BotLevelingUtils
    {
        public static ulong XpNeededForLevel(ulong lvl) => (ulong)(5 * Math.Pow(lvl, 2) + 50 * lvl + 100);
        // public static ulong TotalXpForLevel(ulong level) => (ulong)(5.0f / 6.0f * level * (2 * (ulong)Math.Pow(level,2) + 27 * level + 91));
        public static ulong TotalXpForLevel(ulong x) => (ulong)(5.0f / 6.0f * x * (x + 7.0f) * (2.0f * x + 13.0f));
        public static ulong LevelForTotalXp(ulong totalXp)
        {
            var lvl = (ulong)0;
            var totalXpForCurrentLevel = TotalXpForLevel(lvl+1);
            while (totalXp >= totalXpForCurrentLevel)
            {
                lvl++;
                totalXpForCurrentLevel = TotalXpForLevel(lvl+1);
            }
            return lvl;
        }
        
        // public static ulong LevelForTotalXp(ulong totalXp) => totalXp >= 100 ? 
        //     (ulong)(0.14057f * Math.Pow(1.7321f * Math.Sqrt(3888.0f * Math.Pow(totalXp, 2) + 291600.0f * totalXp - 207025.0f) + 108.0f * totalXp + 4050.0f, 1.0f/3.0f) - 4.5f) + 1
        //     : 0;
        
        public static (ulong, ulong, ulong, ulong) ComputeLevelAndXp(ulong lvl, ulong xp, Action<ulong> cb = null)
        {
            while (xp >= XpNeededForLevel(lvl))
            {
                xp -= XpNeededForLevel(lvl);
                lvl++;
                if (xp < XpNeededForLevel(lvl)) cb?.Invoke(lvl);
            }
            var next = XpNeededForLevel(lvl);
            var totalXp = TotalXpForLevel(lvl) + xp;
            return (lvl, xp, next, totalXp);
        }
    }
    
    public class BotDataService
    {
        // private readonly BotDbContext _dbContext;
        // private readonly IMongoCollection<RankData> _profiles;
        private readonly IMongoCollection<LevelData> _levelData;
        private readonly IMongoCollection<MessageThrottle> _messageThottles;
        private readonly IMongoCollection<UserVoiceStats> _userVoiceStats;
        private readonly IMongoCollection<GuildData> _guildData;

        public BotDataService(
            // BotDbContext dbContext, 
            IDatabaseSettings settings
            )
        {
            // _dbContext = dbContext;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            // _profiles = database.GetCollection<RankData>("RankData");
            _levelData = database.GetCollection<LevelData>("LevelData");
            var indexKeysDefinition = Builders<MessageThrottle>.IndexKeys.Ascending("expiry");
            var indexOptions = new CreateIndexOptions { ExpireAfter = new TimeSpan(0, 0, 60) };
            var indexModel = new CreateIndexModel<MessageThrottle>(indexKeysDefinition, indexOptions);
            _messageThottles = database.GetCollection<MessageThrottle>("MessageThrottles");
            _messageThottles.Indexes.CreateOne(indexModel);
            _userVoiceStats = database.GetCollection<UserVoiceStats>("UserVoiceStats");
            _guildData = database.GetCollection<GuildData>("GuildPreferences");
        }

        public static (ulong, ulong, ulong) ComputeLevelAndXp(ulong lvl, ulong xp, Action<ulong> cb = null)
        {
            while (xp >= BotLevelingUtils.XpNeededForLevel(lvl))
            {
                xp -= BotLevelingUtils.XpNeededForLevel(lvl);
                lvl++;
                if (xp < BotLevelingUtils.XpNeededForLevel(lvl)) cb?.Invoke(lvl);
            }
            var next = BotLevelingUtils.XpNeededForLevel(lvl);
            return (lvl, xp, next);
        }

        public async Task DeleteMessageThrottle(ulong guildId, ulong userId, string key, CancellationToken cancellationToken = default)
        {
            await _messageThottles.DeleteOneAsync(mt =>
                mt.guildId.Equals($"{guildId}") &&
                mt.userId.Equals($"{userId}") &&
                mt.key.Equals(key), cancellationToken: cancellationToken);
        }

        public async Task<MessageThrottle> GetMessageThrottle(ulong guildId, ulong userId, string key, bool canCreate = false, CancellationToken cancellationToken = default)
        {
            var messageThrottle = await _messageThottles.Find(mt =>
                mt.guildId.Equals($"{guildId}") &&
                mt.userId.Equals($"{userId}") &&
                mt.key.Equals(key))
                .FirstOrDefaultAsync();
            if (messageThrottle?.expiry.ToLocalTime() < DateTimeOffset.UtcNow)
            {
                await DeleteMessageThrottle(guildId, userId, key, cancellationToken);
                return null;
            }
            if (messageThrottle != null || !canCreate) return messageThrottle;
            messageThrottle = new MessageThrottle($"{guildId}", $"{userId}", key) {
                expiry = new BsonDateTime(DateTime.UtcNow.AddMinutes(1))
            };
            await _messageThottles.InsertOneAsync(messageThrottle, cancellationToken: cancellationToken);
            return messageThrottle;
        }

        public UserVoiceStats GetUserVoiceStats(ulong guildId, ulong userId)
        {
            var userVoiceStats = _userVoiceStats.Find(ld => ld.guildId.Equals(guildId.ToString()) && ld.userId.Equals(userId.ToString())).FirstOrDefault();
            if (userVoiceStats != null) return userVoiceStats;
            userVoiceStats = new UserVoiceStats
            {
                guildId = $"{guildId}",
                userId = $"{userId}",
                channelId = $"",
                isActive = false,
                totalTimeSpentInVoice = 0,
            };
            _userVoiceStats.InsertOne(userVoiceStats);
            return userVoiceStats;
        }

        public void UpdateUserVoiceStats(UserVoiceStats userVoiceStats)
        {
            _userVoiceStats.ReplaceOne(ld => ld.guildId.Equals(userVoiceStats.guildId) && ld.userId.Equals(userVoiceStats.userId),
                userVoiceStats);
        }

        public LevelData GetLevelData(ulong guildId, ulong userId)
        {
            var userLevelData = _levelData.Find(ld => ld.guildId.Equals(guildId.ToString()) && ld.userId.Equals(userId.ToString())).FirstOrDefault();
            if (userLevelData != null) return userLevelData;
            userLevelData = new LevelData
            {
                guildId = $"{guildId}",
                userId = $"{userId}",
                lastUpdated = DateTimeOffset.MinValue
            };
            _levelData.InsertOne(userLevelData);
            return userLevelData;
        }
        
        public void UpdateUserLevelData(LevelData levelData)
        {
            _levelData.ReplaceOne(ld => ld.guildId.Equals(levelData.guildId) && ld.userId.Equals(levelData.userId),
                levelData);
        }

        public ulong GetUserRank(ulong guildId, ulong userId)
        {
            var x= _levelData.Find(e => e.guildId.Equals(guildId)).SortByDescending(a => a.level).ThenByDescending(a => a.xp).ToList();
            return (ulong)x.FindIndex(a => a.userId.Equals($"{userId}"))+1;
        }

        public LevelData AddMoney(ulong guildId, ulong userId, ulong i)
        {
            var userData = GetLevelData(guildId, userId);
            userData.money += i;
            UpdateUserLevelData(userData);
            return userData;
        }

        public LevelData RemoveMoney(ulong guildId, ulong userId, ulong i)
        {
            var userData = GetLevelData(guildId, userId);
            userData.money = userData.money >= i ? userData.money - i : 0;
            UpdateUserLevelData(userData);
            return userData;
        }
        
        public LevelData AddXp(ulong guildId, ulong userId, ulong i, Action<ulong> cb = null)
        {
            var userData = GetLevelData(guildId, userId);
            userData.xp += (ulong)i;
            (userData.level, userData.xp, _, userData.totalXp) = BotLevelingUtils.ComputeLevelAndXp(userData.level, userData.xp, cb);
            userData.lastUpdated = DateTimeOffset.UtcNow;
            UpdateUserLevelData(userData);
            return userData;
        }

        public LevelData RemoveXp(ulong guildId, ulong userId, ulong amt)
        {
            var userData = GetLevelData(guildId, userId);
            var totalXp = userData.totalXp > 0
                ? userData.totalXp
                : BotLevelingUtils.TotalXpForLevel(userData.level) + userData.xp;
            totalXp -= amt;
            userData.level = BotLevelingUtils.LevelForTotalXp(totalXp);
            userData.xp = totalXp - BotLevelingUtils.TotalXpForLevel(userData.level);
            userData.totalXp = totalXp;
            UpdateUserLevelData(userData);
            return userData;
        }
        
        public void IncrementUserMessageCount(ulong guildId, ulong userId)
        {
            var userData = GetLevelData(guildId, userId);
            if (userData.messageCount <= 0)
            {
                var averageMessages = ( BotLevelingUtils.TotalXpForLevel(userData.level) + userData.xp ) / 18;
                userData.messageCount = averageMessages;
            }
            userData.messageCount++;
            UpdateUserLevelData(userData);
        }

        public GuildData GetGuild(ulong guildId)
        {
            var guildData = _guildData.Find(ld => ld.guildId.Equals(guildId.ToString())).FirstOrDefault();
            if (guildData != null) return guildData;
            guildData = new GuildData
            {
                guildId = $"{guildId}",
                channelNotifications = new Dictionary<string, string>()
            };
            _guildData.InsertOne(guildData);
            return guildData;
        }

        public void UpdateGuild(ulong guildId, GuildData data)
        {
            var guild = GetGuild(guildId);
            var guildUpdate = new GuildData(guild.Id, data);
            UpdateGuild(guildUpdate);
        }

        public void UpdateGuild(GuildData guildData)
        {
            _guildData.ReplaceOne(i => i.Id == guildData.Id, guildData);
            // _guildData.ReplaceOne(ld => ld.guildId.Equals(guildData.guildId), guildData);
        }

        internal async Task<IEnumerable<GuildData>> GetGuildsAsync()
        {
            var result = await _guildData.Find(Builders<GuildData>.Filter.Empty).ToListAsync();
            return result;
        }

        /*
        public List<RankData> Get() => 
            _profiles.Find(profile => true).ToList();

        public RankData Get(string id) =>
            _profiles.Find(rank => rank.Id.Equals(id)).FirstOrDefault();

        public RankData Create(RankData data)
        {
            _profiles.InsertOne(data);
            return data;
        }

        public void Update(string id, RankData @in)
        {
            _profiles.ReplaceOne(rank => rank.Id.Equals(id), @in);
        }

        public void Remove(RankData @in)
        {
            _profiles.DeleteOne(profile => profile.Id.Equals(@in.Id));
        }
        
        public void Remove(string id)
        {
            _profiles.DeleteOne(profile => profile.Id.Equals(id));
        }
         */
    }
}