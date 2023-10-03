﻿using bot.Modules;
using MediatR;
using v10.Data.Abstractions.Models;

namespace bot.Commands;

public class RemoveUserXpCommand : IRequest<LevelData>
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public ulong Amount { get; set; }
    public XpType Type { get; set; } = XpType.Text;
}