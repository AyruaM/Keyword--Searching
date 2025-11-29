using Microsoft.ML.Data;

namespace Keyword_Searching.Models
{
    public class KeywordInput
    {
        public float Volume { get; set; }
        public float TrendAvg { get; set; }
        public float KeywordDifficulty { get; set; }
        public float CompetitiveDensity { get; set; }
        public float NumberOfResults { get; set; }

        [ColumnName("Label")]  // ML.NET label column
        public bool Useful { get; set; }
    }
}
