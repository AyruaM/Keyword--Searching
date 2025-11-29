using System.Text.Json;

public class BioKeywordExpansionService
{
    private readonly List<string> _baseKeywords = new()
    {
        "age","height","wife","girlfriend","father","mother","brother","sister",
        "family","biography","bio","real name","net worth","birthday","birth date",
        "date of birth","bday","tattoo","wiki","wikipedia","film","films","web series",
        "instagram","insta","facebook","twitter","religion","caste"
    };

    private HashSet<string>? _expandedKeywords;

    public async Task<HashSet<string>> GetExpandedKeywordsAsync()
    {
        if (_expandedKeywords != null)
            return _expandedKeywords;

        // ❗️ Replace with actual OpenAI / ChatGPT API call
        var extraKeywords = await GenerateExtraKeywordsWithAI();

        _expandedKeywords = new HashSet<string>(
            _baseKeywords
            .Concat(extraKeywords)
            .Select(k => k.ToLowerInvariant())
        );

        return _expandedKeywords;
    }

    private async Task<List<string>> GenerateExtraKeywordsWithAI()
    {
        // Pretend this is an API call to OpenAI / GPT:
        await Task.Delay(100);

        return new List<string>
        {
            // AI-expanded synonyms, variations, user intent terms:
            "parents", "children", "kids", "sons", "daughters",
            "husband", "partner", "boyfriend",
            "siblings", "relatives",
            "early life", "childhood", "education",
            "career", "profession",
            "movies", "shows", "series",
            "followers", "photos", "pics",
            "ethnicity", "nationality",
            "background", "personal life",
            "married", "marriage"
        };
    }
}
