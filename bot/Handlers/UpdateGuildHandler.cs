using bot.Commands;
using bot.Modules;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;

namespace bot.Handlers;

public class UpdateGuildHandler : IRequestHandler<UpdateGuildCommand, ulong>
{
  private readonly BotDataService _botDataService;

  public UpdateGuildHandler(BotDataService botDataService)
  {
    _botDataService = botDataService;
  }

  public Task<ulong> Handle(UpdateGuildCommand request, CancellationToken cancellationToken)
  {
    var x = new Guild
    {
      guildId = $"{request.GuildId}",
      channelNotifications = request.ChannelNotifications,
    };
    _botDataService.UpdateGuild(request.GuildId, x);
    return Task.FromResult(request.GuildId);
  }
}
