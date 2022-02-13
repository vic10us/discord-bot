using bot.Features.Database;
using Microsoft.Extensions.Configuration;

namespace bot.Features.Images;

public class ImageService
{
    private readonly IConfiguration _config;
    private readonly BotDataService _botDataService;

    public ImageService(IConfiguration config, BotDataService botDataService)
    {
        _config = config;
        _botDataService = botDataService;
    }

    public void GenerateSVG(string template, object data)
    {

    }
}
