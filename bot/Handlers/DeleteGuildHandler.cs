using bot.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.MongoDB;

namespace bot.Handlers;

public class DeleteGuildHandler : IRequestHandler<DeleteGuildCommand, bool>
{
  private readonly BotDataService _botDataService;

  public DeleteGuildHandler(BotDataService botDataService)
  {
    _botDataService = botDataService;
  }

  public async Task<bool> Handle(DeleteGuildCommand request, CancellationToken cancellationToken)
  {
    return await _botDataService.DeleteGuildAsync(request.GuildId);
  }
}