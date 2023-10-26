using LanguageExt.Common;
using v10.Data.Abstractions.Models;

namespace v10.Data.MongoDB;

public interface IBotDataService
{
    LevelData AddMoney(ulong guildId, ulong userId, ulong i);
    LevelData AddVoiceXp(ulong guildId, ulong userId, ulong i, Action<ulong> cb = null);
    LevelData AddXp(ulong guildId, ulong userId, ulong i, Action<ulong> cb = null);
    (ulong, ulong, ulong) ComputeLevelAndXp(ulong lvl, ulong xp, Action<ulong> cb = null);
    Task<Guild> CreateGuildAsync(Guild guild);
    Task<bool> DeleteGuildAsync(ulong guildId);
    Task DeleteMessageThrottle(ulong guildId, ulong userId, string key, CancellationToken cancellationToken = default);
    Guild GetGuild(ulong guildId, bool canCreate = true);
    Task<IEnumerable<Guild>> GetGuildsAsync();
    LevelData GetLevelData(ulong guildId, ulong userId);
    Task<MessageThrottle> GetMessageThrottle(ulong guildId, ulong userId, string key, bool canCreate = false, CancellationToken cancellationToken = default);
    Task<Result<ulong>> GetUserBalance(ulong guildId, ulong userId);
    ulong GetUserRank(ulong guildId, ulong userId);
    UserVoiceStats GetUserVoiceStats(ulong guildId, ulong userId);
    void IncrementUserMessageCount(ulong guildId, ulong userId);
    Task<Result<ulong>> JoinEconomy(ulong guildId, ulong userId);
    Task<Result<bool>> LeaveEconomy(ulong guildId, ulong userId);
    LevelData RemoveMoney(ulong guildId, ulong userId, ulong i);
    LevelData RemoveVoiceXp(ulong guildId, ulong userId, ulong amt, Action<ulong> cb = null);
    LevelData RemoveXp(ulong guildId, ulong userId, ulong amt, Action<ulong> cb = null);
    LevelData SetVoiceXp(ulong guildId, ulong userId, ulong amt, Action<ulong, string> cb = null);
    LevelData SetXp(ulong guildId, ulong userId, ulong amt, Action<ulong, string> cb = null);
    void UpdateGuild(Guild guildData);
    void UpdateGuild(ulong guildId, Guild data);
    Task<bool> UpdateGuildName(ulong guildId, string guildName, CancellationToken cancellationToken);
    void UpdateUserLevelData(LevelData levelData);
    void UpdateUserVoiceStats(UserVoiceStats userVoiceStats);
}
