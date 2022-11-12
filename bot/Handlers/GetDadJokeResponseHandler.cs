using bot.Features.DadJokes;
using bot.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Handlers;

public class GetDadJokeResponseHandler : IRequestHandler<GetDadJokeResponse, DadJoke>
{
    private readonly DadJokeService service;

    public GetDadJokeResponseHandler(DadJokeService service)
    {
        this.service = service;
    }

    public async Task<DadJoke> Handle(GetDadJokeResponse request, CancellationToken cancellationToken)
    {
        return await service.GetDadJoke();
    }
}
