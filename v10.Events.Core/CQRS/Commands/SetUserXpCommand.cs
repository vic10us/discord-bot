using LanguageExt.Common;
using MediatR;
using v10.Data.Abstractions.Models;
using v10.Events.Core.Enums;

namespace v10.Events.Core.Commands;

public class SetUserXpCommand : IRequest<Result<LevelData?>>
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public ulong Amount { get; set; }
    public XpType Type { get; set; } = XpType.Text;
}
