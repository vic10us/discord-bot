using MediatR;
using v10.Data.MongoDB;
using v10.Events.Core.Commands;

namespace v10.Events.Core.CQRS.Handlers;

public class UpdateGuildNameCommandHandler : IRequestHandler<UpdateGuildNameCommand>
{
    private readonly IBotDataService _botDataService;

    public UpdateGuildNameCommandHandler(IBotDataService botDataService)
    {
        _botDataService = botDataService;
    }

    public async Task Handle(UpdateGuildNameCommand request, CancellationToken cancellationToken)
    {
        await _botDataService.UpdateGuildName(StringToUInt64(request.GuildId), request.GuildName, cancellationToken);
    }

    ulong StringToUInt64(string value)
        => ulong.TryParse(value, out var val) ? val : default;
}
