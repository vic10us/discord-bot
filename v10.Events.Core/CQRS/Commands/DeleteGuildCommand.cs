using MediatR;

namespace v10.Events.Core.Commands;

public class DeleteGuildCommand : IRequest<bool>
{
    public ulong GuildId { get; set; }

    public DeleteGuildCommand(ulong guildId)
    {
        GuildId = guildId;
    }
}

