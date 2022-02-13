using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using bot.Features.Database;
using HandlebarsDotNet;

namespace bot.Features.RedneckJokes;

public class RedneckJokeService
{
    private readonly BotDbContext _db;

    public RedneckJokeService(BotDbContext db)
    {
        _db = db;
    }

    private async Task LoadDb()
    {
        if (!_db.Groups.Any(g => g.Category.Equals("RedNeckJokes")))
        {
            var e = _db.Groups.Add(new MessageGroup
            {
                Category = "RedNeckJokes",
                Name = "Red Neck Jokes"
            });
            await _db.SaveChangesAsync();
            e.Entity.Replies.Add(new BotReply
            {

            });
        }
    }

    protected async Task<IEnumerable<string>> GetQuotes()
    {
        var result = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"bot.Features.RedneckJokes.redneckjokes.txt";
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

    public async Task<string> GetQuote()
    {
        var quotes = (await GetQuotes()).ToList();
        var r = new Random();

        var resp = quotes.ElementAt(r.Next(0, quotes.Count));
        return resp;
    }
}
