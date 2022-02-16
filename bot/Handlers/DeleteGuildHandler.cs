using AutoMapper;
using bot.Commands;
using bot.Features.Database;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

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