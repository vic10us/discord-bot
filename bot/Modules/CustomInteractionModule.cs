using System;
using System.IO;
using System.Threading.Tasks;
using bot.Features.Caching;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace bot.Modules;

public class CustomInteractionModule<T> : InteractionModuleBase<T> where T : class, IInteractionContext
{
    internal ILogger _logger;
    internal ICacheContext _cacheContext;

    public override void BeforeExecute(ICommandInfo command)
    {
        base.BeforeExecute(command);
        _cacheContext.SetContext(Context);
    }
}
