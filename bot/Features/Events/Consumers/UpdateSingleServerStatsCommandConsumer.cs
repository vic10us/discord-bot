using System.Threading.Tasks;
using bot.Commands;
using bot.Features.Events.Contracts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace bot.Features.Events.Consumers;

public class UpdateSingleServerStatsCommandConsumer : IConsumer<UpdateSingleServerStatsCommand>
{
    private readonly ILogger<UpdateAllServerStatsCommandConsumer> _logger;
    private readonly IMediator _mediator;

    public UpdateSingleServerStatsCommandConsumer(
        ILogger<UpdateAllServerStatsCommandConsumer> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UpdateSingleServerStatsCommand> context)
    {
        await _mediator.Send(new UpdateGuildStatsCommand(context.Message.GuildId));
    }
}
