using bot.Dtos;
using FluentValidation;

namespace bot.Validation;

public class CreateGuildRequestValidator : AbstractValidator<CreateGuildRequest>
{
  public CreateGuildRequestValidator()
  {
    RuleFor(x => x.GuildId).NotEmpty().GreaterThan((ulong)0).LessThan(ulong.MaxValue);
    RuleFor(x => x.ChannelNotifications).NotNull();
  }
}
