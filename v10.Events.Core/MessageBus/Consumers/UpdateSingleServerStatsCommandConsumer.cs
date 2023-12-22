using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using v10.Events.Core.Commands;
using v10.Events.Core.MessageBus.Contracts;

namespace v10.Events.Core.MessageBus.Consumers;

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
