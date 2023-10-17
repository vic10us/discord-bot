using bot.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.MongoDB;

namespace bot.Handlers;

public class UpdateGuildNameCommandHandler : IRequestHandler<UpdateGuildNameCommand>
{
    private readonly BotDataService _botDataService;

    public UpdateGuildNameCommandHandler(BotDataService botDataService)
    {
        _botDataService = botDataService;
    }

    public async Task Handle(UpdateGuildNameCommand request, CancellationToken cancellationToken)
    {
        await _botDataService.UpdateGuildName(StringToUInt64(request.GuildId), request.GuildName, cancellationToken);
    }

    ulong StringToUInt64(string value)
        => ulong.TryParse(value, out ulong val) ? val : default;
}
