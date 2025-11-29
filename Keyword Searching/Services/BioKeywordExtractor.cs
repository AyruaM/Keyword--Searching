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

        var allKeywords = rows
            .Select(r => new
            {
                Keyword = r.Cell(1).GetValue<string>().Trim(),
                Volume = float.TryParse(r.Cell(3).GetValue<string>(), out var v) ? v : 0
            })
            .ToList();

        const int batchSize = 10;

        for (int i = 0; i < allKeywords.Count; i += batchSize)
        {
            var batch = allKeywords.Skip(i).Take(batchSize).ToList();
            string batchKeywords = string.Join(", ", batch.Select(b => b.Keyword));

            string response = await _ai.IsBioRelatedAsync(batchKeywords);
            var flags = response.Split(',').Select(x => x.Trim().ToLower()).ToList();

            for (int j = 0; j < batch.Count; j++)
            {
                if (j < flags.Count && flags[j] == "yes")
                    results.Add(new KeywordResult(batch[j].Keyword, batch[j].Volume));
            }
        }

        return results;
    }

    public string SaveResultsToExcel(List<KeywordResult> results, string uploadFolder)
    {
        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        // delete old files
        foreach (var file in Directory.GetFiles(uploadFolder))
            File.Delete(file);

        string fileName = $"bio_keywords_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        string filePath = Path.Combine(uploadFolder, fileName);

        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Bio Keywords");

        sheet.Cell(1, 1).Value = "Keyword";
        sheet.Cell(1, 2).Value = "Volume";

        int row = 2;
        foreach (var r in results)
        {
            sheet.Cell(row, 1).Value = r.Keyword;
            sheet.Cell(row, 2).Value = r.Volume;
            row++;
        }

        sheet.Columns().AdjustToContents();
        workbook.SaveAs(filePath);

        return filePath;
    }



    public record KeywordResult(string Keyword, float Volume);

}
