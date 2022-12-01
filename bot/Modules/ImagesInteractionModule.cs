using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using bot.Features.Pictures;
using bot.Queries;
using bot.Services;
using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.OpenApi.Extensions;

namespace bot.Modules;

public class ImagesInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    private static ImageType GetRandomImageType()
    {
        var types = Enum.GetValues<ImageType>().Where(t => t != ImageType.Random).ToArray();
        var random = new Random();
        var position = random.Next(0, types.Length);
        return types[position];
    }

    public ImagesInteractionModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetChoiceDisplayName(Enum enumValue)
    {
        // ChoiceDisplayAttribute
        var attribute = enumValue.GetAttributeOfType<ChoiceDisplayAttribute>();
        return attribute == null ? enumValue.ToString() : attribute.Name;
    }

    [SlashCommand("image", "Get an image")]
    public async Task GetAnImage(ImageType imageType)
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
        await RespondWithFileAsync(stream, publicFileName, embed: embed);
    }
}
