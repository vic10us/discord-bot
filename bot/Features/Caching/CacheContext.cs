using bot.Modules;
using LanguageExt.Common;
using StackExchange.Redis;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Discord.Commands;
using Discord;

namespace bot.Features.Caching;

public class CacheContext<T> : ICacheContext where T : class
{
    internal RedisKey RedisKey => $"_{typeof(T).Name}_MessageReceived_{ContextId}";

    internal string ContextId
    {
        get
        {
            if (Context is ICommandContext)
                return (Context as ICommandContext).Message.Id.ToString();

            return (Context as IInteractionContext).Interaction.Id.ToString();
        }
    }

    internal static RedisValue RedisToken => $"{Environment.MachineName}-{Guid.NewGuid()}";

    public T Context { get; private set; }

    void ICacheContext.SetContext(ICommandContext context)
    {
        T val = context as T;
        Context = val ?? throw new InvalidOperationException($"Invalid context type. Expected {typeof(T).Name}, got {context.GetType().Name}.");
    }

    void ICacheContext.SetContext(IInteractionContext context)
    {
        T val = context as T;
        Context = val ?? throw new InvalidOperationException($"Invalid context type. Expected {typeof(T).Name}, got {context.GetType().Name}.");
    }

    private readonly IDatabase _database;
    private readonly ILogger _logger;

    public CacheContext(IDatabase database, ILogger logger)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// Release the lock on the message.
    /// </summary>
    public void ReleaseLock()
    {
        if (_database == null) { return; }
        _database.LockRelease(RedisKey, RedisToken);
    }

    /// <summary>
    /// Acquire a lock on the message to ensure that only one instance of the command is running at a time.
    /// </summary>
    /// <returns></returns>
    public bool EnsureSingle()
    {
        if (_database == null) { return true; }
        _logger.LogInformation("Acquiring Lock: {RedisKey} {RedisToken}", RedisKey, RedisToken);
        var lockTaken = _database.LockTake(RedisKey, RedisToken, TimeSpan.FromSeconds(1));
        if (!lockTaken)
        {
            _logger.LogWarning("{ClassName} Message is already being processed {ContextId}", typeof(T).Name, ContextId);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Run an action with a lock on the message to ensure that only one instance of the command is running at a time.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task WithLock(Func<Task> action)
    {
        if (_database == null) { await action(); return; }
        var lockTaken = false;
        try
        {
            _logger.LogInformation("Acquiring Lock: {RedisKey} {RedisToken}", RedisKey, RedisToken);
            lockTaken = await _database.LockTakeAsync(RedisKey, RedisToken, TimeSpan.FromSeconds(1));
            if (!lockTaken)
            {
                _logger.LogWarning("{ClassName} Message is already being processed {ContextId}", typeof(T).Name, ContextId);
                return;
            }

            await action();
        }
        finally
        {
            if (lockTaken)
            {
                _database.LockRelease(RedisKey, RedisToken);
            }
        }
    }

    // Wrap WithLock in a Result so we can return errors
    async Task<Result<bool>> WithLockResult(Func<Task> action)
    {
        if (_database == null) { await action(); return true; }
        var lockTaken = false;
        try
        {
            _logger.LogInformation("Acquiring Lock: {RedisKey} {RedisToken}", RedisKey, RedisToken);
            lockTaken = await _database.LockTakeAsync(RedisKey, RedisToken, TimeSpan.FromSeconds(1));
            if (!lockTaken)
            {
                _logger.LogWarning("{ClassName} Message is already being processed {ContextId}", typeof(T).Name, ContextId);
                return new Result<bool>(new DuplicateMessageException($"{typeof(T).Name} Message is already being processed {ContextId}"));
            }

            await action();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running {ClassName}", typeof(T).Name);
            return new Result<bool>(ex);
        }
        finally
        {
            if (lockTaken)
            {
                _database.LockRelease(RedisKey, RedisToken);
            }
        }
    }

    /// <summary>
    /// Run an action with a lock on the message to ensure that only one instance of the command is running at a time.
    /// </summary>
    /// <param name="action">
    /// The action to run with the lock.
    /// </param>
    /// <param name="timeout">
    /// The expiry time for the lock.
    /// The default is 1 second.
    /// </param>
    /// <returns>
    /// The result of the action as a <see cref="Result{R}"/>.
    /// </returns>
    public async Task<Result<R>> WithLock<R>(Func<Task<R>> action, TimeSpan? timeout = null)
    {
        if (_database == null) { return await action(); }
        var lockTaken = false;
        try
        {
            _logger.LogInformation("Acquiring Lock: {RedisKey} {RedisToken}", RedisKey, RedisToken);
            lockTaken = await _database.LockTakeAsync(RedisKey, RedisToken, timeout ?? TimeSpan.FromSeconds(1));
            if (!lockTaken)
            {
                _logger.LogWarning("{ClassName} Message is already being processed {ContextId}", typeof(T).Name, ContextId);
                return new Result<R>(new DuplicateMessageException($"{typeof(T).Name} Message is already being processed {ContextId}"));
            }

            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running {ClassName}", typeof(T).Name);
            return new Result<R>(ex);
        }
        finally
        {
            if (lockTaken)
            {
                _database.LockRelease(RedisKey, RedisToken);
            }
        }
    }

}
