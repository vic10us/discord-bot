using Discord.Rest;

namespace BotAdminUI.Services;

public interface IDiscordUserService
{
    DiscordRestClient Client { get; }
    DiscordRestClient BotClient { get; }
    Task<IEnumerable<RestUserGuild>> GetGuildsAsync();
    Task<IEnumerable<RestUserGuild>> GetInstalledGuilds();
    Task<RestUser> GetUserAsync();
    Task<string?> GetTokenAsync();
}
