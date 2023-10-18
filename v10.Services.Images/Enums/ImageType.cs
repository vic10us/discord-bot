using System.ComponentModel.DataAnnotations;
using Discord.Interactions;

namespace v10.Services.Images.Enums;

public enum ImageType
{
    [Display(Name = "Cat Image")]
    [ChoiceDisplay("Cat")]
    Cat,
    [Display(Name = "Bunny Image")]
    [ChoiceDisplay("Bunny")]
    Bunny,
    [Display(Name = "Bunny & Cat Combo Image")]
    [ChoiceDisplay("Bunny & Cat Combo")]
    BunnyCat,
    [Display(Name = "Sea Creature Image")]
    [ChoiceDisplay("Sea Creature")]
    SeaCreature,
    [ChoiceDisplay("Random")]
    Random
}
