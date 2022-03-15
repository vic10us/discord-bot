using bot.Dtos;
using FluentValidation;

namespace bot.Validation;

public class UpdateGuildRequestValidator : AbstractValidator<UpdateGuildRequest>
{
  public UpdateGuildRequestValidator()
  {
    RuleFor(x => x.GuildId).NotEmpty().GreaterThan((ulong)0).LessThan(ulong.MaxValue);
    RuleFor(x => x.ChannelNotifications).NotNull();
  }
}
