using System;
using System.IO;
using System.Threading.Tasks;
using bot.Features.Caching;
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
        var database = server.Multiplexer.GetDatabase();
        _logger = logger;
        _imageService = imageService;
        _pictureService = pictureService;
        _cacheContext = new CacheContext<SocketCommandContext>(database, logger);
    }

    [Command("bunny")]
    public async Task GetBunny()
    {
        await _cacheContext.WithLock(async () =>
        {
            var (filename, stream) = await _pictureService.GetPictureFromCategory("bunny");
            stream.Seek(0, SeekOrigin.Begin);
            var ext = Path.GetExtension(filename);
            await SendImageEmbed(stream, "Random Bunny", $"bunny.{ext}", Color.Green);
        });
    }

    [Command("seacreature")]
    [Alias("sc", "creature")]
    public async Task GetSeaCreature()
    {
        await _cacheContext.WithLock(async () =>
        {
            var (filename, stream) = await _pictureService.GetPictureFromCategory("seacreature");
            stream.Seek(0, SeekOrigin.Begin);
            var ext = Path.GetExtension(filename);
            await SendImageEmbed(stream, "Random Sea Creature", $"seacreature.{ext}", Color.Green);
        });
    }

    [Command("qrcode")]
    [Alias("qr")]
    public async Task GetQRCode(IUser user = null)
    {
        await _cacheContext.WithLock(async () =>
        {
            user ??= Context.User;
            var imageStream = await _imageService.CreateQRCode();
            // var image = System.Drawing.Image.FromStream(imageStream);
            await SendImageEmbed(imageStream, $"{user.Username}#{user.Discriminator} QR Code", "qrcode.png", Color.Blue);
        });
    }

    [Command("bunnycat")]
    [Alias("bc")]
    public async Task GetBunnyCat()
    {
        await _cacheContext.WithLock(async () =>
        {
            var (filename, stream) = await _pictureService.GetPictureFromCategory("bunnycat");
            stream.Seek(0, SeekOrigin.Begin);
            var ext = Path.GetExtension(filename);
            await SendImageEmbed(stream, "Random Bunny with Cat", $"bunnycat.{ext}", Color.Green);
        });
    }

    [Command("cat")]
    public async Task CatAsync()
    {
        await _cacheContext.WithLock(async () =>
        {            // Get a stream containing an image of a cat
            var stream = await _pictureService.GetCatPictureAsync();
            await SendImageEmbed(stream, "Random Cat", $"cat.png", Color.Green);
        });
    }
}
