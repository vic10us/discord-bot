using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.IO;
using System.Threading.Tasks;

namespace bot.Modules;

public abstract class CustomModule<T> : ModuleBase<T> where T : class, ICommandContext
{
    internal RedisKey RedisKey => $"_{GetType().Name}_MessageReceived_{Context.Message.Id}";
    internal RedisValue RedisToken => $"{Environment.MachineName}-{Guid.NewGuid()}";

    internal IDatabase _database;
    internal ILogger _logger;

    protected virtual void ReleaseLock()
    {
        if (_database == null) { return; }
        _database.LockRelease(RedisKey, RedisToken);
    }

    protected virtual bool EnsureSingle()
    {
        if (_database == null) { return true; }
        _logger.LogInformation("Acquiring Lock: {RedisKey} {RedisToken}", RedisKey, RedisToken);
        var lockTaken = _database.LockTake(RedisKey, RedisToken, TimeSpan.FromSeconds(1));
        if (!lockTaken)
        {
            _logger.LogWarning("{Name} Message is already being processed {Content}", GetType().Name, Context.Message.Id);
            return false;
        }

        return true;
    }

    protected virtual async Task<IUserMessage> ReplyAsyncEx(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null)
    {
        return await Context.Channel.SendMessageAsync(message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds).ConfigureAwait(false);
    }

    protected virtual async Task SendImageEmbed(Stream fileStream, string title, string filename, Color color)
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithImageUrl($"attachment://{filename}")
            .WithColor(color)
            .Build();
        await Context.Channel.SendFileAsync(stream: fileStream, filename: filename, embed: embed);
    }
}
