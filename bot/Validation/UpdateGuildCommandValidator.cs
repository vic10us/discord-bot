using bot.Commands;
using FluentValidation;

namespace bot.Validation;

public class UpdateGuildCommandValidator : AbstractValidator<UpdateGuildCommand>
{
    public UpdateGuildCommandValidator()
    {
        Transform(x => x.GuildId, StringToUInt64)
            .NotNull()
            .GreaterThan((ulong)0)
            .LessThan(ulong.MaxValue);
        RuleFor(x => x.ChannelNotifications)
            .NotNull();
    }

    ulong StringToUInt64(string value)
        => ulong.TryParse(value, out ulong val) ? val : default;
}
