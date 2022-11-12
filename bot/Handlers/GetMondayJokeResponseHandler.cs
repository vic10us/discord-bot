using bot.Features.MondayQuotes;
using bot.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Handlers;

public class GetMondayJokeResponseHandler : IRequestHandler<GetMondayJokeResponse, string>
{
    private readonly MondayQuotesService service;

    public GetMondayJokeResponseHandler(MondayQuotesService service)
    {
        this.service = service;
    }

    public async Task<string> Handle(GetMondayJokeResponse request, CancellationToken cancellationToken)
    {
        return await service.GetQuote();
    }
}
