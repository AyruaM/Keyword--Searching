using Microsoft.ML.Data;

namespace Keyword_Searching.Models
{
    public class KeywordPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Useful { get; set; }

        public float Score { get; set; }
    }
}
