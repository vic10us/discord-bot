using LanguageExt.Common;
using MediatR;
using v10.Data.MongoDB;
using v10.Events.Core.Commands;

namespace v10.Events.Core.CQRS.Handlers;

public class DeleteGuildHandler : IRequestHandler<DeleteGuildCommand, Result<bool>>
{
    private readonly IBotDataService _botDataService;

    public DeleteGuildHandler(IBotDataService botDataService)
    {
        _botDataService = botDataService;
    }

    public async Task<Result<bool>> Handle(DeleteGuildCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _botDataService.DeleteGuildAsync(request.GuildId);
        }
        catch (Exception ex)
        {
            return new Result<bool>(ex);
        }
    }
}
