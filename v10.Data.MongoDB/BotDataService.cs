using System;
using LanguageExt.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using v10.Bot.Core;
using v10.Data.Abstractions.Interfaces;
using v10.Data.Abstractions.Models;

namespace v10.Data.MongoDB;

public class BotDataService
{
    private readonly IMongoCollection<LevelData> _levelData;
    private readonly IMongoCollection<MessageThrottle> _messageThottles;
    private readonly IMongoCollection<UserVoiceStats> _userVoiceStats;
    private readonly IMongoCollection<Guild> _guilds;

    public BotDataService(IDatabaseSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        _messageThottles = database.GetCollectionWithExpiry<MessageThrottle>("MessageThrottles");
        _levelData = database.GetCollection<LevelData>("LevelData");
        _userVoiceStats = database.GetCollection<UserVoiceStats>("UserVoiceStats");
        _guilds = database.GetCollection<Guild>("Guilds");
    }

    public async Task<Result<ulong>> GetUserBalance(ulong guildId, ulong userId)
    {
        try
        {
            var userLevelData = (await _levelData.FindAsync(ld => ld.guildId.Equals(guildId.ToString()) && ld.userId.Equals(userId.ToString()))).FirstOrDefault()?.money ?? 0;
            return userLevelData;
        }
        catch (Exception ex)
        {
            return new Result<ulong>(ex);
        }
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

    public async Task<Result<bool>> LeaveEconomy(ulong guildId, ulong userId)
    {
        try
        {
            await _levelData.DeleteOneAsync(f => f.guildId.Equals(guildId.ToString()) && f.userId.Equals(userId.ToString()));
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(ex);
        }
    }

    public async Task<Result<ulong>> JoinEconomy(ulong guildId, ulong userId)
    {
        // if user is already a member of economy then do nothing and return their balance
        // if user is not already a member of the economy then add them and give them 500 coins

        var userLevelData = (await _levelData.FindAsync(ld => ld.guildId.Equals(guildId.ToString()) && ld.userId.Equals(userId.ToString()))).FirstOrDefault();
        if (userLevelData != null) return new Result<ulong>(userLevelData.money);
        AddMoney(guildId, userId, 500);
        return new Result<ulong>(500);
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
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (messageThrottle?.expiry.ToLocalTime() < DateTimeOffset.UtcNow)
        {
            await DeleteMessageThrottle(guildId, userId, key, cancellationToken);
            return null;
        }
        if (messageThrottle != null || !canCreate) return messageThrottle;
        messageThrottle = new MessageThrottle($"{guildId}", $"{userId}", key)
        {
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
        var x = _levelData.Find(e => e.guildId.Equals(guildId)).SortByDescending(a => a.level).ThenByDescending(a => a.xp).ToList();
        return (ulong)x.FindIndex(a => a.userId.Equals($"{userId}")) + 1;
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
        userData.xp += i;
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
            var averageMessages = (BotLevelingUtils.TotalXpForLevel(userData.level) + userData.xp) / 18;
            userData.messageCount = averageMessages;
        }
        userData.messageCount++;
        UpdateUserLevelData(userData);
    }

    public Guild GetGuild(ulong guildId, bool canCreate = true)
    {
        var guildData = _guilds.Find(ld => ld.guildId.Equals(guildId.ToString())).FirstOrDefault();
        if (guildData != null) return guildData;
        if (!canCreate) return null;
        guildData = new Guild
        {
            guildId = $"{guildId}",
            channelNotifications = new Dictionary<string, string>()
        };
        _guilds.InsertOne(guildData);
        return guildData;
    }

    public void UpdateGuild(ulong guildId, Guild data)
    {
        var guild = GetGuild(guildId);
        var guildUpdate = new Guild(guild.Id, data);
        UpdateGuild(guildUpdate);
    }

    public void UpdateGuild(Guild guildData)
    {
        _guilds.ReplaceOne(i => i.Id == guildData.Id, guildData);
        // _guildData.ReplaceOne(ld => ld.guildId.Equals(guildData.guildId), guildData);
    }

    public async Task<IEnumerable<Guild>> GetGuildsAsync()
    {
        var result = await _guilds.Find(Builders<Guild>.Filter.Empty).ToListAsync();
        return result;
    }

    public async Task<Guild> CreateGuildAsync(Guild guild)
    {
        var x = await _guilds.FindAsync((g) => g.guildId.Equals(guild.guildId));
        if (x.FirstOrDefault() != null) throw new Exception("Guild with that GuildId already exists");
        var guildData = new Guild
        {
            guildId = guild.guildId,
            channelNotifications = guild.channelNotifications,
        };
        await _guilds.InsertOneAsync(guildData);
        return guildData;
    }

    public async Task<bool> DeleteGuildAsync(ulong guildId)
    {
        var result = await _guilds.DeleteOneAsync(g => g.guildId.Equals($"{guildId}"));
        return result.IsAcknowledged && result.DeletedCount > 0;
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
