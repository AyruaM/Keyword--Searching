namespace KeywordFilteringML.Models
{
    public class KeywordData
    {
        public string Keyword { get; set; }
        public string Intent { get; set; }
        public float Volume { get; set; }
        public string Trend { get; set; }
        public float KeywordDifficulty { get; set; }
        public float CompetitiveDensity { get; set; }
        public string SERPFeatures { get; set; }
        public bool Useful { get; set; }
        public float NumberOfResults { get; set; }
    }
}
