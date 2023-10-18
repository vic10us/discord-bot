using MediatR;
using v10.Services.RedneckJokes.Queries;

namespace v10.Services.RedneckJokes.Handlers;

public class GetRedneckJokeResponseHandler : IRequestHandler<GetRedneckJokeResponse, string>
{
    private readonly IRedneckJokeService service;

    public GetRedneckJokeResponseHandler(IRedneckJokeService service)
    {
        this.service = service;
    }

    public async Task<string> Handle(GetRedneckJokeResponse request, CancellationToken cancellationToken)
    {
        return await service.GetQuote();
    }
}
