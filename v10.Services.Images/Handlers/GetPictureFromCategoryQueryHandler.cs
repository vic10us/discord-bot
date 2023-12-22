using MediatR;
using v10.Services.Images.Queries;

namespace v10.Services.Images.Handlers;

public class GetPictureFromCategoryQueryHandler : IRequestHandler<GetPictureFromCategoryQuery, (string, Stream)>
{
    private readonly IPictureService _pictureService;

    public GetPictureFromCategoryQueryHandler(IPictureService pictureService)
    {
        _pictureService = pictureService;
    }

    public async Task<(string, Stream)> Handle(GetPictureFromCategoryQuery request, CancellationToken cancellationToken)
    {
        var (fileName, stream) = await _pictureService.GetPictureFromCategory(request.ImageType.ToString());
        return (fileName, stream);
    }
}
