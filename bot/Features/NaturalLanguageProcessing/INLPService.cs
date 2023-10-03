using System.Collections.Generic;

namespace bot.Features.NaturalLanguageProcessing;

public interface INLPService
{
    IEnumerable<string> GetSentences(string input);
}
