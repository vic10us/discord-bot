using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace bot.Features.NaturalLanguageProcessing;

public static class StringExtensions
{
    public static IEnumerable<string> ExtractSentences(this string input)
    {
        string pattern = @"([^!.?]+[.!?])|([^!.?]+)\s*";

        MatchCollection matches = Regex.Matches(input, pattern);
        
        for (int i = 0; i < matches.Count; i++)
        {
            yield return matches[i].Value.Trim();
        }
    }

    //public static IEnumerable<string> ParseSentences(this string input)
    //{
    //    var sentences = input.ExtractSentences();
    //    foreach (var sentence in sentences)
    //    {
    //        yield return sentence;
    //    }
    //}
}
