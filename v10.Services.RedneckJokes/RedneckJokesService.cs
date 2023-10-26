using System.Reflection;

namespace v10.Services.RedneckJokes;

public class RedneckJokeService : IRedneckJokeService
{
    public RedneckJokeService()
    {
    }

    protected async Task<IEnumerable<string>> GetQuotes()
    {
        var result = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.Data.redneckjokes.txt";
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

    public async Task<string> GetQuote()
    {
        var quotes = (await GetQuotes()).ToList();
        var r = new Random();

        var resp = quotes.ElementAt(r.Next(0, quotes.Count));
        return resp;
    }
}
