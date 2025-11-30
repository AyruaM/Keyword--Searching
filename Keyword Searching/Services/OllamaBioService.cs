using System.Net.Http.Json;

public class OllamaBioService
{
    private readonly HttpClient _http;

    public OllamaBioService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ollama");
    }

    public async Task<string> IsBioRelatedAsync(string keywordsCsv)
    {
        var keywords = keywordsCsv
            .Split(',')
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToList();

        // Heuristics: customize these lists to match your policy
        var personIndicators = new[]
        {
        "age", "birthday", "born", "net worth", "wife", "husband", "spouse",
        "parents", "mother", "father", "children", "son", "daughter",
        "biography", "bio", "wiki", "profile"
    };

        // Treat appearance/style words as NOT bio-related per your request
        var appearanceIndicators = new[]
        {
        "hair", "hairstyle", "hair style", "beard", "look", "looks",
        "style", "outfit", "dress", "clothes", "tattoo", "piercing", "makeup", "images"
        ,"mp3","lyrics", "download" ,"song"  };

        // Prepare arrays to keep final answers in input order
        var finalAnswers = new string[keywords.Count]; // will contain "YES" or "NO"
        var toClassify = new List<(int index, string keyword)>();

        // Pre-classify using heuristics (fast, deterministic)
        for (int i = 0; i < keywords.Count; i++)
        {
            var k = keywords[i].ToLowerInvariant();

            // If contains explicit person indicator -> YES
            if (personIndicators.Any(p => k.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                finalAnswers[i] = "YES";
                continue;
            }

            // If contains appearance/style indicator -> NO (as you requested)
            if (appearanceIndicators.Any(a => k.Contains(a, StringComparison.OrdinalIgnoreCase)))
            {
                finalAnswers[i] = "NO";
                continue;
            }

            // else mark for model classification
            toClassify.Add((i, keywords[i]));
        }

        // If everything classified by rules, return
        if (toClassify.Count == 0)
        {
            return string.Join(",", finalAnswers);
        }

        // Build model input only for undecided items
        var modelKeywords = toClassify.Select(t => t.keyword).ToList();
        string numberedList = string.Join("\n", modelKeywords.Select((k, idx) => $"{idx + 1}. {k}"));

        string prompt = $@"
You are a classification machine.

Classify each keyword as YES or NO based on this rule:

YES = The keyword describes a person's biography or personal attributes  
(age, birth, birthplace, birthday, family, parents, spouse, children, siblings, education, occupation, background, wiki, biography, net worth).

NO =please return NO for any song or song related keywords or Anything else. If unsure → NO.

Output EXACTLY {{N}} lines.
Each line format: ""<index>. YES"" or ""<index>. NO""
No extra words. No explanations.

Keywords:{numberedList}
";

        var body = new
        {
            model = "llama3.1:8b",
            messages = new[]
            {
            new Dictionary<string,string>
            {
                { "role", "user" },
                { "content", prompt }
            }
        },
            stream = false,
            temperature = 0,
            max_tokens = modelKeywords.Count * 4 + 20
        };

        var response = await _http.PostAsJsonAsync("/api/chat", body);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();
        string raw = json?.Message?.Content?.Trim() ?? "";

        // Parse model output into YES/NO list
        var lines = raw.Split('\n')
                       .Select(l => l.Trim())
                       .Where(l => !string.IsNullOrWhiteSpace(l))
                       .ToList();

        var modelAnswers = new List<string>();
        foreach (var line in lines)
        {
            var parts = line.Split(new[] { '.', ')', ':', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var token = parts.Last().Trim().ToUpperInvariant();
            if (token == "YES" || token == "NO")
                modelAnswers.Add(token);
        }

        // Fix count mismatches defensively
        while (modelAnswers.Count < modelKeywords.Count) modelAnswers.Add("NO");
        if (modelAnswers.Count > modelKeywords.Count) modelAnswers = modelAnswers.Take(modelKeywords.Count).ToList();

        // Merge model answers back into finalAnswers in original order
        for (int m = 0; m < toClassify.Count; m++)
        {
            var originalIndex = toClassify[m].index;
            finalAnswers[originalIndex] = modelAnswers[m];
        }

        return string.Join(",", finalAnswers);
    }


}


