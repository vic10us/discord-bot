using Microsoft.Extensions.Configuration;

namespace v10.Services.Images;

public class PictureService : IPictureService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public PictureService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<Stream> GetCatPictureAsync()
    {
        var resp = await _http.GetAsync("https://cataas.com/cat");
        return await resp.Content.ReadAsStreamAsync();
    }

    private static string getRandomFileFromPath(string path)
    {
        string file = null;
        if (string.IsNullOrEmpty(path)) return file;
        var fullPath = Path.GetFullPath(path);
        var extensions = new string[] { ".png", ".jpg", ".gif", ".webp" };
        try
        {
            var di = new DirectoryInfo(fullPath);
            var rgFiles = di.GetFiles("*.*").Where(f => extensions.Contains(f.Extension.ToLower()));
            var r = new Random();
            var fileInfos = rgFiles.ToList();
            file = fileInfos.ElementAt(r.Next(0, fileInfos.Count)).FullName;
        }
        // probably should only catch specific exceptions
        // throwable by the above methods.
        catch { }
        return file;
    }

    public async Task<(string fileName, Stream fileStream)> GetPictureFromCategory(string category)
    {
        if (category.Equals("cat", StringComparison.InvariantCultureIgnoreCase)) return ($"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_cat.png", await GetCatPictureAsync());
        var path = _config[$"ImagePaths:{category}"];
        var imageFileName = getRandomFileFromPath(path);
        if (imageFileName == null) throw new ArgumentException("Could not find any pictures!");
        return (imageFileName, File.Open(imageFileName, FileMode.Open, FileAccess.Read));
    }
}
