using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace bot.Features.StrangeLaws;

public class StrangeLawsService : IStrangeLawsService
{
    private readonly string[] _strangeLaws;
    private readonly List<string> _cache = new();

    public StrangeLawsService()
    {
        _strangeLaws = GetStrangeLaws().Result.ToArray();
    }

    protected async Task<IEnumerable<string>> GetStrangeLaws()
    {
        var result = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"bot.Features.StrangeLaws.data.txt";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using (var reader = new StreamReader(stream))
        {
            string lines;
            while ((lines = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(lines)) continue;
                result.Add(lines);
            }
        }

        return result;
    }

    public Task<string> Get()
    {
        try
        {
            Console.WriteLine($"Cache contains {_cache.Count}");
            var quotes = _strangeLaws.Except(_cache).ToList();

            // var quotes = (await GetStrangeLaws()).ToList();

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
            Console.WriteLine(e);
            throw;
        }
    }
}
