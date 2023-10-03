using System.Collections.Generic;

namespace bot.Features.NaturalLanguageProcessing;

public class NLPService : INLPService
{
    public IEnumerable<string> GetSentences(string input)
    {
        var sentencesList = input.ExtractSentences();
        return sentencesList;
    }
}
