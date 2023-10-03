using Discord.Interactions;

namespace bot.Modules;

public enum XpType
{
    [ChoiceDisplay("Text Xp")]
    Text,
    [ChoiceDisplay("Voice Xp")]
    Voice
}
