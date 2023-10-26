using MediatR;
using v10.Services.StrangeLaws.Queries;

namespace v10.Services.StrangeLaws.Handlers;

public class GetStrangeLawResponseHandler : IRequestHandler<GetStrangeLawResponse, string>
{
    private readonly IStrangeLawsService service;

    public GetStrangeLawResponseHandler(IStrangeLawsService service)
    {
        this.service = service;
    }

    public async Task<string> Handle(GetStrangeLawResponse request, CancellationToken cancellationToken)
    {
        return await service.Get();
    }
}
