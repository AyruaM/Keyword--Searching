using ClosedXML.Excel;

public class BioKeywordExtractor
{
    private readonly OllamaBioService _ai;

    public BioKeywordExtractor(OllamaBioService ai)
    {
        _ai = ai;
    }

    public async Task<List<KeywordResult>> ExtractBioKeywordsAsync(string filePath)
    {
        var results = new List<KeywordResult>();

        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var rows = sheet.RangeUsed().RowsUsed().Skip(1);

        // Collect all keywords first
        var allKeywords = rows
            .Select(r => new
            {
                Keyword = r.Cell(1).GetValue<string>().Trim(),
                Volume = float.TryParse(r.Cell(3).GetValue<string>(), out var v) ? v : 0
            })
            .ToList();

        // Process in batches of 10
        const int batchSize = 10;

        for (int i = 0; i < allKeywords.Count; i += batchSize)
        {
            var batch = allKeywords.Skip(i).Take(batchSize).ToList();

            // Create comma-separated keyword list
            string batchKeywords = string.Join(", ", batch.Select(b => b.Keyword));

            // Call your AI method once for all 10 keywords
            // Response example: "yes,no,yes,yes,no,no,..."
            string response = await _ai.IsBioRelatedAsync(batchKeywords);

            // Split response
            var flags = response.Split(',')
                                .Select(x => x.Trim().ToLower())
                                .ToList();

            // Map each yes/no to the corresponding keyword
            for (int j = 0; j < batch.Count; j++)
            {
                if (j < flags.Count && flags[j] == "yes")
                {
                    results.Add(new KeywordResult(batch[j].Keyword, batch[j].Volume));
                }
            }
        }

        return results;
    }

    public record KeywordResult(string Keyword, float Volume);

}
