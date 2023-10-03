using bot.Features.StrangeLaws;
using bot.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Handlers;

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
