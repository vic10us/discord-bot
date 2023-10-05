using MediatR;
using v10.Games.EightBall.Models;
using v10.Games.EightBall.Queries;

namespace v10.Games.EightBall.Handlers;

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
