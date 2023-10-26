using System;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace bot.Modules;

public class CustomInteractionModule<T> : InteractionModuleBase<T> where T : class, IInteractionContext
{
    internal RedisKey RedisKey => $"_{GetType().Name}_MessageReceived_{Context.Interaction.Id}";
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
            var q = _database.LockQuery(RedisKey);
            _logger.LogWarning("{Name} Message is already being processed {Content} {val}", GetType().Name, Context.Interaction.Id, q);
            return false;
        }

        return true;
    }
}
