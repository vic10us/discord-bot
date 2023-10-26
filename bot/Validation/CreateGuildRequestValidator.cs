using FluentValidation;
using v10.Events.Core.Dtos;

namespace bot.Validation;

public class CreateGuildRequestValidator : AbstractValidator<CreateGuildRequest>
{
    public CreateGuildRequestValidator()
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
