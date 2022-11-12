using Discord.Interactions;

namespace bot.Modules;

public enum JokeType
{
    [ChoiceDisplay("Dad joke")]
    Dad,
    [ChoiceDisplay("Monday joke")]
    Monday,
    [ChoiceDisplay("Redneck joke")]
    Redneck,
    [ChoiceDisplay("Pick for me! (random)")]
    Random
}
