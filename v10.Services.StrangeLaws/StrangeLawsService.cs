using System.Reflection;
using Microsoft.Extensions.Logging;

namespace v10.Services.StrangeLaws;

public class StrangeLawsService : IStrangeLawsService
{
    private readonly string[] _strangeLaws;
    private readonly List<string> _cache = new();
    private readonly ILogger<StrangeLawsService> _logger;

    public StrangeLawsService(ILogger<StrangeLawsService> logger)
    {
        _strangeLaws = GetStrangeLaws().Result.ToArray();
        _logger = logger;
    }

    protected async Task<IEnumerable<string>> GetStrangeLaws()
    {
        var result = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.Data.data.txt";
        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Resource {resourceName} not found in assembly {assembly.GetName().Name}.");
        using var reader = new StreamReader(stream);
        
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            result.Add(line);
        }

        return result;
    }

    public Task<string> Get()
    {
        try
        {
            _logger.LogInformation($"Cache contains {_cache.Count}");
            var quotes = _strangeLaws.Except(_cache).ToList();

            var r = new Random();

            var resp = quotes.ElementAt(r.Next(0, quotes.Count));

            _cache.Add(resp);
            if (_cache.Count > _strangeLaws.Length * 0.75)
            {
                _cache.RemoveAt(0);
            }

            return Task.FromResult(resp);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting strange law");
            throw;
        }
    }
}
