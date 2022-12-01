using bot.Features.Pictures;
using bot.Queries;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Handlers;

public class GetPictureFromCategoryQueryHandler : IRequestHandler<GetPictureFromCategoryQuery, (string, Stream)>
{
    private readonly PictureService _pictureService;

    public GetPictureFromCategoryQueryHandler(PictureService pictureService)
    {
        _pictureService = pictureService;
    }

    public async Task<(string, Stream)> Handle(GetPictureFromCategoryQuery request, CancellationToken cancellationToken)
    {
        var (fileName, stream) = await _pictureService.GetPictureFromCategory(request.ImageType.ToString());
        return (fileName, stream);
    }
}
