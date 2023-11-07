using System.Security.Cryptography;
using System.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Data.MongoDB;

namespace v10.Bot.Discord;

public class DiscordMessageService : IDiscordMessageService
{
    private readonly ILogger<DiscordMessageService> _logger;
    private readonly IDatabase _database;
    private readonly IBotDataService _botDataService;
    private readonly DiscordSocketClient _discordSocketClient;

    public DiscordMessageService(
        IBotDataService botDataService,
        DiscordSocketClient discordSocketClient,
        IServiceProvider serviceProvider,
        ILogger<DiscordMessageService> logger
        )
    {
        _logger = logger;
        _database = serviceProvider.GetRequiredService<IServer>().Multiplexer.GetDatabase();
        _botDataService = botDataService;
        _discordSocketClient = discordSocketClient;
    }

    static string ComputeSha256Hash(string rawData)
    {
        // Create a SHA256
        // ComputeHash - returns byte array
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

        // Convert byte array to a string
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

    public async Task SendMessageAsync(ulong guildId, string route, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, CancellationToken cancellationToken = default)
    {
        var messageHash = ComputeSha256Hash($"{guildId}{route}{message}{isTTS}{embed?.GetHashCode()}{options?.GetHashCode()}{allowedMentions?.GetHashCode()}{messageReference?.GetHashCode()}{components?.GetHashCode()}{stickers?.GetHashCode()}{embeds?.GetHashCode()}{flags}");
        RedisKey RedisKey = $"_SendMessageAsync_{messageHash}";
        RedisValue RedisToken = $"{Environment.MachineName}-{Guid.NewGuid()}";
        _logger.LogInformation("Acquiring Lock: {RedisKey} {RedisToken}", RedisKey, RedisToken);
        var lockTaken = _database.LockTake(RedisKey, RedisToken, TimeSpan.FromSeconds(1));
        if (!lockTaken)
        {
            _logger.LogWarning("Message is already being processed {Content}", messageHash);
            return;
        }
        try
        {
            var guildDataResult = _botDataService.GetGuild(guildId);
            guildDataResult.IfSucc(async (guildData) => {
                if (guildData == null) return;
                if (!guildData.channelNotifications.TryGetValue(route, out var channelId_str)) return;
                if (string.IsNullOrWhiteSpace(channelId_str)) return;
                if (!ulong.TryParse(channelId_str, out var channelId)) return;
                await SendMessageAsync(channelId, message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags, cancellationToken);
            });
        }
        finally
        {
            _database.LockRelease(RedisKey, RedisToken);
        }
    }

    private async Task SendMessageAsync(ulong channelId, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, CancellationToken cancellationToken = default)
    {
        if (_discordSocketClient.GetChannel(channelId) is not IMessageChannel channel) return;
        await channel.SendMessageAsync(message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
    }
}
