using Discord.Interactions;

namespace v10.Events.Core.Enums;

public enum JokeType
{
    [ChoiceDisplay("Dad joke")]
    Dad,
    [ChoiceDisplay("Monday joke")]
    Monday,
    [ChoiceDisplay("Redneck joke")]
    Redneck,
    [ChoiceDisplay("Random Strange Laws")]
    StrangeLaw,
    [ChoiceDisplay("Pick for me! (random)")]
    Random
}

