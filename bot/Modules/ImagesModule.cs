using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using v10.Services.Images;

namespace bot.Modules;

public class ImagesModule : CustomModule<SocketCommandContext>
{
    private readonly IImageApiService _imageService;
    private readonly IPictureService _pictureService;

    public ImagesModule(
        ILogger<ImagesModule> logger,
        IServiceProvider serviceProvider,
        IImageApiService imageService,
        IPictureService pictureService
        )
    {
        var server = serviceProvider.GetRequiredService<IServer>();
        _database = server.Multiplexer.GetDatabase();
        _logger = logger;
        _imageService = imageService;
        _pictureService = pictureService;
    }

    [Command("bunny")]
    public async Task GetBunny()
    {
        if (!EnsureSingle()) { return; }
        try
        {
            var (filename, stream) = await _pictureService.GetPictureFromCategory("bunny");
            stream.Seek(0, SeekOrigin.Begin);
            var ext = Path.GetExtension(filename);
            await SendImageEmbed(stream, "Random Bunny", $"bunny.{ext}", Color.Green);
        }
        finally
        {
            ReleaseLock();
        }
    }

    [Command("seacreature")]
    [Alias("sc", "creature")]
    public async Task GetSeaCreature()
    {
        if (!EnsureSingle()) { return; }
        try
        {
            var (filename, stream) = await _pictureService.GetPictureFromCategory("seacreature");
            stream.Seek(0, SeekOrigin.Begin);
            var ext = Path.GetExtension(filename);
            await SendImageEmbed(stream, "Random Sea Creature", $"seacreature.{ext}", Color.Green);
        }
        finally
        {
            ReleaseLock();
        }
    }

    [Command("qrcode")]
    [Alias("qr")]
    public async Task GetQRCode(IUser user = null)
    {
        if (!EnsureSingle()) { return; }
        try
        {
            user ??= Context.User;
            var imageStream = await _imageService.CreateQRCode();
            // var image = System.Drawing.Image.FromStream(imageStream);
            await SendImageEmbed(imageStream, $"{user.Username}#{user.Discriminator} QR Code", "qrcode.png", Color.Blue);
        }
        finally
        {
            ReleaseLock();
        }
    }

    [Command("bunnycat")]
    [Alias("bc")]
    public async Task GetBunnyCat()
    {
        if (!EnsureSingle()) { return; }
        try
        {
            var (filename, stream) = await _pictureService.GetPictureFromCategory("bunnycat");
            stream.Seek(0, SeekOrigin.Begin);
            var ext = Path.GetExtension(filename);
            await SendImageEmbed(stream, "Random Bunny with Cat", $"bunnycat.{ext}", Color.Green);
        }
        finally
        {
            ReleaseLock();
        }
    }

    [Command("cat")]
    public async Task CatAsync()
    {
        if (!EnsureSingle()) { return; }
        try
        {
            // Get a stream containing an image of a cat
            var stream = await _pictureService.GetCatPictureAsync();
            await SendImageEmbed(stream, "Random Cat", $"cat.png", Color.Green);
        } 
        finally
        {
            ReleaseLock();
        }
    }
}
