using System.Reflection;
using v10.Services.MondayQuotes.Enums;

namespace v10.Services.MondayQuotes;

public class MondayQuotesService : IMondayQuotesService
{
    protected static async Task<IEnumerable<string>> GetQuotes(QuoteCategory category = QuoteCategory.Funny)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.Data.MondayQuotes.{category}.txt";
        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Resource {resourceName} not found in assembly {assembly.GetName().Name}.");
        using var reader = new StreamReader(stream);

        var result = new List<string>();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            result.Add(line);
        }

        return result;
    }

    public async Task<string> GetQuote(QuoteCategory category = QuoteCategory.Funny)
    {
        var quotes = (await GetQuotes(category)).ToList();
        var r = new Random();

        var resp = quotes.ElementAt(r.Next(0, quotes.Count));
        return resp;
    }
}
