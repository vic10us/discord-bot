﻿using Discord.Interactions;

namespace bot.Modules;

public enum XpOperationType
{
    [ChoiceDisplay("Add XP")]
    Add,
    [ChoiceDisplay("Remove XP")]
    Remove,
    [ChoiceDisplay("Set XP")]
    Set
}