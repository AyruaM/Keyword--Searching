using ExcelDataReader;
using Keyword_Searching.Models;
using KeywordFilteringML.Models;

namespace Keyword_Searching.Utils
{
    public static class KeywordFileParser
    {
        public static List<KeywordData> ParseCsv(string[] lines)
        {
            var list = new List<KeywordData>();

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var c = line.Split('\t');

                if (c.Length < 8)
                    continue;

                float volume = float.TryParse(c[2].Trim(), out var v) ? v : 0f;
                float kd = float.TryParse(c[4].Trim(), out var kdVal) ? kdVal : 0f;
                float cd = float.TryParse(c[5].Trim(), out var cdVal) ? cdVal : 0f;
                float results = float.TryParse(c[7].Trim(), out var r) ? r : 0f;

                list.Add(new KeywordData
                {
                    Keyword = c[0].Trim(),
                    Volume = volume,
                    Trend = c[3].Trim(),
                    KeywordDifficulty = kd,
                    CompetitiveDensity = cd,
                    NumberOfResults = results
                });
            }

            return list;
        }

        public static List<KeywordData> ParseExcel(Stream stream)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var ds = reader.AsDataSet();

            var table = ds.Tables[0];
            var result = new List<KeywordData>();

            for (int i = 1; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                float volume = float.TryParse(row[2].ToString(), out var v) ? v : 0f;
                float kd = float.TryParse(row[4].ToString(), out var kdVal) ? kdVal : 0f;
                float cd = float.TryParse(row[5].ToString(), out var cdVal) ? cdVal : 0f;
                float results = float.TryParse(row[7].ToString(), out var r) ? r : 0f;

                result.Add(new KeywordData
                {
                    Keyword = row[0].ToString()?.Trim(),
                    Volume = volume,
                    Trend = row[3].ToString(),
                    KeywordDifficulty = kd,
                    CompetitiveDensity = cd,
                    NumberOfResults = results
                });
            }

            return result;
        }
    }
}
