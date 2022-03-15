using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace bot.Features.MondayQuotes;

public class MondayQuotesService
{
  protected async Task<IEnumerable<string>> GetQuotes(QuoteCategory category = QuoteCategory.Funny)
  {
    var result = new List<string>();
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = $"bot.Features.MondayQuotes.{category}.txt";
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

    //var result = reader.ReadToEnd();
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
