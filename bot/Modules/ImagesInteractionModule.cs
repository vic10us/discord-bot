using System;
using System.IO;
using System.Threading.Tasks;
using bot.Features.Caching;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Extensions;
using StackExchange.Redis;
using v10.Services.Images.Enums;
using v10.Services.Images.Queries;

namespace bot.Modules;

public class ImagesInteractionModule : CustomInteractionModule<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public ImagesInteractionModule(
        IMediator mediator, 
        ILogger<ImagesInteractionModule> logger,
        IServiceProvider serviceProvider
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        var database = server.Multiplexer.GetDatabase();
        _mediator = mediator;
        _logger = logger;
        _cacheContext = new CacheContext<SocketInteractionContext>(database, logger);
    }

    private static string GetChoiceDisplayName(Enum enumValue)
    {
        var attribute = enumValue.GetAttributeOfType<ChoiceDisplayAttribute>();
        return attribute == null ? enumValue.ToString() : attribute.Name;
    }

    [SlashCommand("image", "Get an image")]
    public async Task GetAnImage(ImageType imageType)
    {
        await DeferAsync();
        await _cacheContext.WithLock(async () =>
        {
            var query = new GetPictureFromCategoryQuery(imageType);
            var (fileName, stream) = await _mediator.Send(query);
            var ext = Path.GetExtension(fileName);
            var publicFileName = $"{imageType}.{ext}";
            var embed = new EmbedBuilder()
                .WithTitle($"Random {GetChoiceDisplayName(imageType)} Image")
                .WithImageUrl($"attachment://{publicFileName}")
                .WithColor(Color.Green)
                .Build();
            await FollowupWithFileAsync(stream, publicFileName, embed: embed);
        });
    }
}
