using MediatR;

namespace bot.Commands;

public class DeleteGuildCommand : IRequest<bool>
{
    public ulong GuildId { get; set; }

    public DeleteGuildCommand(ulong guildId)
    {
        GuildId = guildId;
    }
}
