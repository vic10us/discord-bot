using MediatR;
using v10.Services.DadJokes;
using v10.Services.DadJokes.Models;
using v10.Services.DadJokes.Queries;

namespace bot.Handlers;

public class GetDadJokeResponseHandler : IRequestHandler<GetDadJokeResponse, IDadJoke>
{
    private readonly IDadJokeService service;

    public GetDadJokeResponseHandler(IDadJokeService service)
    {
        this.service = service;
    }

    public async Task<IDadJoke> Handle(GetDadJokeResponse request, CancellationToken cancellationToken)
    {
        return await service.GetJokeAsync();
    }
}
