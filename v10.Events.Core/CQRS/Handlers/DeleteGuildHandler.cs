using MediatR;
using v10.Data.MongoDB;
using v10.Events.Core.Commands;

namespace v10.Events.Core.CQRS.Handlers;

public class DeleteGuildHandler : IRequestHandler<DeleteGuildCommand, bool>
{
    private readonly IBotDataService _botDataService;

    public DeleteGuildHandler(IBotDataService botDataService)
    {
        _botDataService = botDataService;
    }

    public async Task<bool> Handle(DeleteGuildCommand request, CancellationToken cancellationToken)
    {
        return await _botDataService.DeleteGuildAsync(request.GuildId);
    }
}
