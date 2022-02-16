using bot.Commands;
using FluentValidation;

namespace bot.Validation;

public class UpdateGuildCommandValidator : AbstractValidator<UpdateGuildCommand>
{
    public UpdateGuildCommandValidator()
    {
        RuleFor(x => x.GuildId).NotEmpty().GreaterThan((ulong)0).LessThan(ulong.MaxValue);
        RuleFor(x => x.ChannelNotifications).NotNull();
    }
}
