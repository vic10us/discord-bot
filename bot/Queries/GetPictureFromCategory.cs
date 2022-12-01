using System.IO;
using bot.Modules;
using MediatR;

namespace bot.Queries;

public class GetPictureFromCategoryQuery : IRequest<(string, Stream)> 
{
    public ImageType ImageType { get; init; }

    public GetPictureFromCategoryQuery(ImageType imageType)
    {
        this.ImageType = imageType;
    }
}
