﻿using FluentValidation;
using v10.Events.Core.Commands;

namespace bot.Validation;

public class UpdateGuildCommandValidator : AbstractValidator<UpdateGuildCommand>
{
    public UpdateGuildCommandValidator()
    {
        RuleFor(x => StringToUInt64(x.GuildId))
            .GreaterThan((ulong)0)
            .LessThan(ulong.MaxValue)
            .OverridePropertyName("GuildId");

        RuleFor(x => x.ChannelNotifications)
            .NotNull();
    }

    ulong StringToUInt64(string value)
        => ulong.TryParse(value, out ulong val) ? val : default;
}
