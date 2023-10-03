using System.Text.Json;

enum TranslationUsage
{
    Formal,
    Informal
}

record Translation(string Text, TranslationUsage Usage);

class Program
{
    static void Main(string[] args)
    {
        Dictionary<string, List<Translation>> translations = LoadTranslationsFromJson("translations.json");
        Console.OutputEncoding = System.Text.Encoding.Unicode;

        var all = translations
                    .SelectMany(kv => kv.Value.Select(t => new { Language = kv.Key, Translation = t }))
                    .GroupBy(t => t.Language)
                    .ToDictionary(g => g.Key, g => g.Select(v => v.Translation.Text));

        foreach (var item in all)
        {
            Console.WriteLine(item.Key);
            Console.WriteLine($"\t {item.Value.Aggregate((a,b) => $"{a}, {b}")}");
        }

        //var groupedTranslations = translations
        //    .SelectMany(kv => kv.Value.Select(t => new { Language = kv.Key, Translation = t }))
        //    .GroupBy(t => t.Translation.Usage)
        //    .ToDictionary(
        //                    g => g.Key, 
        //                    g => g.GroupBy(t => t.Language)
        //                          .ToDictionary(r => r.Key, r => r.Select(s => s.Translation))
        //                 );

        //// Example usage
        //var formalTranslations = groupedTranslations[TranslationUsage.Informal];
        //Console.WriteLine("Formal Translations:");
        //foreach (var translation in formalTranslations)
        //{
        //    var values = translation.Value.Select(t => t.Text).ToArray();
        //    var valueText = values.Aggregate((a, b) => $"{a}, {b}");
        //    Console.WriteLine($"Language: {translation.Key}, Translation: {valueText}");
        //}
    }

    static Dictionary<string, List<Translation>> LoadTranslationsFromJson(string filePath)
    {
        try
        {
            string jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var jsonDocument = JsonDocument.Parse(jsonString);
            var translations = new Dictionary<string, List<Translation>>();

            foreach (var language in jsonDocument.RootElement.EnumerateObject())
            {
                var languageTranslations = new List<Translation>();

                foreach (var translation in language.Value.EnumerateArray())
                {
                    var text = translation[0].GetString();
                    var usage = translation[1].GetString();
                    var translationUsage = ParseTranslationUsage(usage);

                    languageTranslations.Add(new Translation(text, translationUsage));
                }

                translations.Add(language.Name, languageTranslations);
            }

            return translations;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading translations from JSON: {ex.Message}");
            return new Dictionary<string, List<Translation>>();
        }
    }

    static TranslationUsage ParseTranslationUsage(string usage)
    {
        return usage.ToLower() switch
        {
            "formal" => TranslationUsage.Formal,
            "informal" => TranslationUsage.Informal,
            _ => throw new ArgumentException($"Invalid translation usage: {usage}")
        };
    }
}
