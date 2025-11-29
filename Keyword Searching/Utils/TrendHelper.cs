namespace Keyword_Searching.Utils
{
    public static class TrendHelper
    {
        public static float ParseTrend(string trend)
        {
            if (string.IsNullOrWhiteSpace(trend))
                return 0f;

            var parts = trend.Split(',');

            var values = parts
                .Select(p => float.TryParse(p.Trim(), out var val) ? val : 0f)
                .ToArray();

            if (values.Length == 0) return 0f;

            return values.Average();
        }
    }
}
