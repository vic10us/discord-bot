using MediatR;

namespace bot.Commands;

public record UpdateGuildStatsCommand(ulong GuildId) : IRequest;
