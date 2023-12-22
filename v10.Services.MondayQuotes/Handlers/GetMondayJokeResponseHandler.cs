using MediatR;
using v10.Services.MondayQuotes;
using v10.Services.MondayQuotes.Queries;

namespace bot.Handlers;

public class GetMondayJokeResponseHandler : IRequestHandler<GetMondayJokeResponse, string>
{
    private readonly IMondayQuotesService service;

    public GetMondayJokeResponseHandler(IMondayQuotesService service)
    {
        this.service = service;
    }

    public async Task<string> Handle(GetMondayJokeResponse request, CancellationToken cancellationToken)
    {
        return await service.GetQuote();
    }
}
