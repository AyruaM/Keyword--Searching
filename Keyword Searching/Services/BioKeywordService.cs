using ClosedXML.Excel;
using System.Text.RegularExpressions;
using static KeywordController;

public class BioKeywordService
{
    private readonly List<string> _baseBioKeywords = new()
    {
        "age","height","wife","girlfriend","father","mother","brother","sister",
        "family","biography","bio","real name","net worth","birthday","birth date",
        "date of birth","bday","tattoo","wiki","wikipedia","film","films","web series",
        "instagram","insta","facebook","twitter","religion","caste"
    };

    private readonly List<string> _aiGeneratedKeywords = new()
    {
        "parents","children","kids","sons","daughters","husband","partner","boyfriend",
        "siblings","relatives","early life","childhood","education","school","college",
        "career","profession","movies","shows","series","followers","photos","pics",
        "ethnicity","nationality","background","personal life","married","marriage"
    };

    // Objects to exclude (user searching "iPhone age" is NOT bio related)
    private readonly List<string> _objectWords = new()
    {
        "iphone","samsung","car","dog","cat","laptop","bike","mobile","phone",
        "country","city","state","device","computer","animal"
    };

    // AI-like person name detector
    private bool SeemsLikePersonName(string keyword)
    {
        // If contains numbers, not a person
        if (Regex.IsMatch(keyword, @"\d")) return false;

        // Look for two-word proper nouns (very common in names)
        var words = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int capitalizedCount = words.Count(w => char.IsUpper(w[0]));
        if (capitalizedCount >= 2) return true;

        // If contains known surnames
        var surnamePatterns = new[] { "khan", "singh", "patel", "smith", "johnson", "musk", "gates", "modi", "sharma" };
        if (surnamePatterns.Any(s => keyword.Contains(s, StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }

    // AI-like BIO classifier
    private bool IsBioRelated(string keyword)
    {
        var low = keyword.ToLower();

        // rule 1 — explicit bio keywords
        if (_baseBioKeywords.Any(k => low.Contains(k))) return true;
        if (_aiGeneratedKeywords.Any(k => low.Contains(k))) return true;

        // rule 2 — exclude objects
        if (_objectWords.Any(obj => low.Contains(obj))) return false;

        // rule 3 — must contain a person's name or look like a PUBLIC FIGURE query
        if (!SeemsLikePersonName(keyword))
            return false;

        // rule 4 — fuzzy patterns (AI-like intent detection)
        string[] bioIntents =
        {
            "how tall", "how old", "who is", "what is the age",
            "where was", "when was", "did .* marry", "early life"
        };

        if (bioIntents.Any(intent => Regex.IsMatch(low, intent)))
            return true;

        // If name detected and query has personal context
        if (low.Contains("life") || low.Contains("story") || low.Contains("info"))
            return true;

        return false;
    }

    public List<KeywordResult> ExtractBioKeywords(string filePath)
    {
        var results = new List<KeywordResult>();

        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var rows = sheet.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            string keyword = row.Cell(1).GetValue<string>().Trim();
            float volume = float.TryParse(row.Cell(3).GetValue<string>(), out var v) ? v : 0;

            if (IsBioRelated(keyword))
                results.Add(new KeywordResult(keyword, volume));
        }

        return results;
    }
}
