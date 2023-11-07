using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;

namespace BotAdminUI.Services;

public class DiscordUserService : IDiscordUserService
{
    private readonly DiscordRestClient _client;
    private readonly DiscordRestClient _botClient;

    public DiscordRestClient Client => _client;
    public DiscordRestClient BotClient => _botClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DiscordUserService([FromKeyedServices("UserClient")] DiscordRestClient discordRestClient, IHttpContextAccessor httpContextAccessor, IServiceProvider services)
    {
        _client = discordRestClient;
        _botClient = services.GetRequiredKeyedService<DiscordRestClient>("BotClient");
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (!(httpContext?.User?.Identity?.IsAuthenticated ?? false))
        {
            return null;
        }

        var tk = await httpContext!.GetTokenAsync("Discord", "access_token");
        return tk;
    }

    public async Task<IEnumerable<RestUserGuild>> GetInstalledGuilds()
    {
        var guilds = await _botClient.GetGuildSummariesAsync().FlattenAsync().ConfigureAwait(false);
        return guilds;
    }

    public async Task<IEnumerable<RestUserGuild>> GetGuildsAsync()
    {
        var userGuilds = await _client.GetGuildSummariesAsync().FlattenAsync().ConfigureAwait(false);
        return userGuilds;
    }

    public async Task<RestUser> GetUserAsync()
    {
        var user = await _client.GetCurrentUserAsync().ConfigureAwait(false);
        return user;
    }

    public static string? GetUserAvatarUrl(string userId, string avatarId, ushort size = 128, ImageFormat format = ImageFormat.Auto)
    {
        if (avatarId == null)
        {
            return null;
        }

        string value = FormatToExtension(format, avatarId);
        return $"{"https://cdn.discordapp.com/"}avatars/{userId}/{avatarId}.{value}?size={size}";
    }

    public static string? GetGuildUserAvatarUrl(string userId, string guildId, string avatarId, ushort size = 128, ImageFormat format = ImageFormat.Auto)
    {
        if (avatarId == null)
        {
            return null;
        }

        string value = FormatToExtension(format, avatarId);
        return $"{"https://cdn.discordapp.com/"}guilds/{guildId}/users/{userId}/avatars/{avatarId}.{value}?size={size}";
    }

    private static string FormatToExtension(ImageFormat format, string imageId)
    {
        if (format == ImageFormat.Auto)
        {
            format = (imageId.StartsWith("a_") ? ImageFormat.Gif : ImageFormat.Png);
        }

        return format switch
        {
            ImageFormat.Gif => "gif",
            ImageFormat.Jpeg => "jpeg",
            ImageFormat.Png => "png",
            ImageFormat.WebP => "webp",
            _ => throw new ArgumentException("format"),
        };
    }
}
