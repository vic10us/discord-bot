using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using LanguageExt.Common;

namespace bot.Features.Caching;

public interface ICacheContext
{
    bool EnsureSingle();
    void ReleaseLock();
    void SetContext(ICommandContext context);
    void SetContext(IInteractionContext context);
    Task WithLock(Func<Task> action);
    Task<Result<R>> WithLock<R>(Func<Task<R>> action, TimeSpan? timeout = null);
}
