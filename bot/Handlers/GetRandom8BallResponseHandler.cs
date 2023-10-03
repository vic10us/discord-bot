using bot.Features.EightBall;
using bot.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Handlers;

public class GetRandom8BallResponseHandler : IRequestHandler<GetRandom8BallResponse, EightBallResponse>
{
    private readonly EightBallService _service;

    public GetRandom8BallResponseHandler(EightBallService service)
    {
        this._service = service;
    }

    public Task<EightBallResponse> Handle(GetRandom8BallResponse request, CancellationToken cancellationToken)
    {
        var response = _service.GetRandomResponse();
        return Task.FromResult(response);
    }
}
