using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace bot.Features.Pictures;

public class PictureService
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

  private string getrandomfile2(string path)
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

  public (string, Stream) GetPictureFromCategory(string category)
  {
    var path = _config[$"ImagePaths:{category}"];
    var randomBunny = getrandomfile2(path);
    if (randomBunny == null) throw new ArgumentException("Could not find any pictures!");
    return (randomBunny, File.Open(randomBunny, FileMode.Open, FileAccess.Read));
  }
}
