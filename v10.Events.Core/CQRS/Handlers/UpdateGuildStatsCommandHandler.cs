using Discord.WebSocket;
using Discord;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using v10.Data.MongoDB;
using v10.Events.Core.Commands;

namespace v10.Events.Core.CQRS.Handlers;

public class UpdateGuildStatsCommandHandler : IRequestHandler<UpdateGuildStatsCommand>
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UpdateGuildStatsCommandHandler> _logger;
    private readonly IBotDataService _botDataService;

    public UpdateGuildStatsCommandHandler(
            DiscordSocketClient discordSocketClient,
            IDistributedCache cache,
            ILogger<UpdateGuildStatsCommandHandler> logger,
            IBotDataService botDataService
        )
    {
        _discordSocketClient = discordSocketClient;
        _cache = cache;
        _logger = logger;
        _botDataService = botDataService;
    }

    public async Task Handle(UpdateGuildStatsCommand request, CancellationToken cancellationToken)
    {
        var guild = _discordSocketClient.GetGuild(request.GuildId);
        if (guild is null)
        {
            _logger.LogWarning("Attempt to update guild with Id {GuildId} was cancelled. [GUILD_NOT_FOUND]", request.GuildId);
            return;
        }

        var cacheKey = $"updated-guild:{request.GuildId}";
        var updatedGuildCacheValue = await _cache.GetStringAsync(cacheKey, token: cancellationToken);

        // Key exists, so we're rate limited
        if (!string.IsNullOrWhiteSpace(updatedGuildCacheValue))
        {
            _logger.LogWarning("Attempt to update guild with Id {GuildId} was cancelled. [RATE_LIMTED]", request.GuildId);
            return;
        }

        var guildData = _botDataService.GetGuild(request.GuildId);
        if (guildData is null)
        {
            _logger.LogWarning("Attempt to update guild with Id {GuildId} was cancelled. [GUILD_NOT_FOUND]", request.GuildId);
            return;
        }

        await _cache.SetStringAsync(cacheKey, "true", new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
        }, token: cancellationToken);

        try
        {
            var memberCount = guild.Users.Count(x => !x.IsBot);
            var onlineCount = guild.Users.Count(x => !x.IsBot && !new[] { UserStatus.Offline, UserStatus.Invisible }.Contains(x.Status));

            var staffCount = guild.Users.Count(x => !x.IsBot && x.Roles.Any(y => guildData.staffRoles.Contains(y.Id.ToString())));
            var onlineStaffCount = guild.Users.Count(x => !x.IsBot && !new[] { UserStatus.Offline, UserStatus.Invisible }.Contains(x.Status) && x.Roles.Any(y => guildData.staffRoles.Contains(y.Id.ToString())));
            var boosts = guild.PremiumSubscriptionCount;
            var boostLevel = guild.PremiumTier;
            var donators = guild.Users.Count(x => !x.IsBot && x.Roles.Any(y => y.Name.ToLower().Contains("donator")));

            guildData.channelNotifications.TryGetValue("stats.current-goal", out var statCurrentGoalChannelId);
            guildData.channelNotifications.TryGetValue("stats.members", out var statMembersChannelId);
            guildData.channelNotifications.TryGetValue("stats.online-members", out var statOnlineChannelId);
            guildData.channelNotifications.TryGetValue("stats.online-staff", out var statOnlineStaffChannelId);
            guildData.channelNotifications.TryGetValue("stats.boosts", out var statBoostsChannelId);
            guildData.channelNotifications.TryGetValue("stats.donators", out var statDonatorsChannelId);

            _logger.LogInformation("Updating guild stats for {Name} ({Id}) {guild}", guild.Name, guild.Id, guild);

            if (!string.IsNullOrWhiteSpace(statCurrentGoalChannelId))
            {
                var statCurrentGoalChannel = guild.GetVoiceChannel(ulong.Parse(statCurrentGoalChannelId));
                await statCurrentGoalChannel.ModifyAsync(prop => prop.Name = $"🎯 Current Goal: {guild.MemberCount} Members");
            }

            if (!string.IsNullOrWhiteSpace(statMembersChannelId))
            {
                var statMembersChannel = guild.GetVoiceChannel(ulong.Parse(statMembersChannelId));
                await statMembersChannel.ModifyAsync(prop => prop.Name = $"👥 Members: {memberCount}");
            }

            if (!string.IsNullOrWhiteSpace(statOnlineChannelId))
            {
                var statOnlineChannel = guild.GetVoiceChannel(ulong.Parse(statOnlineChannelId));
                await statOnlineChannel.ModifyAsync(prop => prop.Name = $"🟢 Online: {onlineCount}");
            }

            if (!string.IsNullOrWhiteSpace(statOnlineStaffChannelId))
            {
                var statOnlineStaffChannel = guild.GetVoiceChannel(ulong.Parse(statOnlineStaffChannelId));
                await statOnlineStaffChannel.ModifyAsync(prop => prop.Name = $"💼 Online Staff: {onlineStaffCount}/{staffCount}");
            }

            if (!string.IsNullOrWhiteSpace(statBoostsChannelId))
            {
                var statBoostsChannel = guild.GetVoiceChannel(ulong.Parse(statBoostsChannelId));
                await statBoostsChannel.ModifyAsync(prop => prop.Name = $"🚀 Boosts: {boosts} ({boostLevel})");
            }

            if (!string.IsNullOrWhiteSpace(statDonatorsChannelId))
            {
                var statDonatorsChannel = guild.GetVoiceChannel(ulong.Parse(statDonatorsChannelId));
                await statDonatorsChannel.ModifyAsync(prop => prop.Name = $"💰 Donators: {donators}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating guild stats [{ex.Message}]", ex);
        }
    }

}
