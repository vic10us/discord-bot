using bot.Commands;
using FluentValidation;

namespace bot.Validation;

public class CreateGuildCommandValidator : AbstractValidator<CreateGuildCommand>
{
  public CreateGuildCommandValidator()
  {
    RuleFor(x => x.GuildId).NotEmpty().GreaterThan((ulong)0).LessThan(ulong.MaxValue);
    RuleFor(x => x.ChannelNotifications).NotNull();
  }
}
