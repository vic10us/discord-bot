using MediatR;

namespace v10.Events.Core.Commands;

public record UpdateGuildStatsCommand(ulong GuildId) : IRequest;
