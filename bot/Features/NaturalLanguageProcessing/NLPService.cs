using System.Collections.Generic;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using java.util;
using System.Linq;

namespace bot.Features.NaturalLanguageProcessing;

public class NLPService : INLPService
{
    private readonly StanfordCoreNLP _stanfordCoreNLP;

    public NLPService(StanfordCoreNLP stanfordCoreNLP)
    {
        _stanfordCoreNLP = stanfordCoreNLP;
    }

    public IEnumerable<string> GetSentences(string input)
    {
        var annotation = new Annotation(input);

        _stanfordCoreNLP.annotate(annotation);

        if (annotation.get(typeof(CoreAnnotations.SentencesAnnotation)) is not ArrayList sentences)
        {
            return new List<string>();
        }

        var sentencesList = sentences.toArray().ToList().Select(e => $"{e}");
        return sentencesList;
    }
}
