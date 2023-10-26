using Discord.Interactions;

namespace v10.Events.Core.Enums;

public enum XpOperationType
{
    [ChoiceDisplay("Add XP")]
    Add,
    [ChoiceDisplay("Remove XP")]
    Remove,
    [ChoiceDisplay("Set XP")]
    Set
}
