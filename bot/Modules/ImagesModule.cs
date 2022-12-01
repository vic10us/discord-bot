using System.IO;
using System.Threading.Tasks;
using bot.Features.Pictures;
using bot.Services;
using Discord;
using Discord.Commands;

namespace bot.Modules;

public class ImagesModule : CustomModule<SocketCommandContext>
{
    public ImageApiService ImageService { get; set; }
    public PictureService PictureService { get; set; }

    [Command("bunny")]
    public async Task GetBunny()
    {
        var (filename, stream) = await PictureService.GetPictureFromCategory("bunny");
        stream.Seek(0, SeekOrigin.Begin);
        var ext = Path.GetExtension(filename);
        await SendImageEmbed(stream, "Random Bunny", $"bunny.{ext}", Color.Green);
    }

    [Command("seacreature")]
    [Alias("sc", "creature")]
    public async Task GetSeaCreature()
    {
        var (filename, stream) = await PictureService.GetPictureFromCategory("seacreature");
        stream.Seek(0, SeekOrigin.Begin);
        var ext = Path.GetExtension(filename);
        await SendImageEmbed(stream, "Random Sea Creature", $"seacreature.{ext}", Color.Green);
    }

    [Command("qrcode")]
    [Alias("qr")]
    public async Task GetQRCode(IUser user = null)
    {
        user ??= Context.User;
        var imageStream = await ImageService.CreateQRCode();
        // var image = System.Drawing.Image.FromStream(imageStream);
        await SendImageEmbed(imageStream, $"{user.Username}#{user.Discriminator} QR Code", "qrcode.png", Color.Blue);
    }

    [Command("bunnycat")]
    [Alias("bc")]
    public async Task GetBunnyCat()
    {
        var (filename, stream) = await PictureService.GetPictureFromCategory("bunnycat");
        stream.Seek(0, SeekOrigin.Begin);
        var ext = Path.GetExtension(filename);
        await SendImageEmbed(stream, "Random Bunny with Cat", $"bunnycat.{ext}", Color.Green);
    }

    [Command("cat")]
    public async Task CatAsync()
    {
        // Get a stream containing an image of a cat
        var stream = await PictureService.GetCatPictureAsync();
        await SendImageEmbed(stream, "Random Cat", $"cat.png", Color.Green);
    }
}
