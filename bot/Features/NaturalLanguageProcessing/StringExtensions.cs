using System.Text.RegularExpressions;

namespace bot.Features.NaturalLanguageProcessing;

public static class StringExtensions
{
    public static string[] ExtractSentences(this string input)
    {
        string pattern = @"([^.!?]+[.!?])";

        MatchCollection matches = Regex.Matches(input, pattern);

        string[] sentences = new string[matches.Count];

        for (int i = 0; i < matches.Count; i++)
        {
            sentences[i] = matches[i].Value.Trim();
        }

        return sentences;
    }
}
