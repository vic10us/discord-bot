using System.IO;
using MediatR;
using v10.Services.Images.Enums;

namespace v10.Services.Images.Queries;

public class GetPictureFromCategoryQuery : IRequest<(string, Stream)>
{
    public ImageType ImageType { get; init; }

    public GetPictureFromCategoryQuery(ImageType imageType)
    {
        ImageType = imageType;
    }
}
