using ClosedXML.Excel;
using Keyword_Searching.Models;
using Keyword_Searching.Utils;
using KeywordFilteringML.Models;
using Microsoft.ML;
using static KeywordController;

public class KeywordMLService
{
    private readonly MLContext _ml;
    private PredictionEngine<KeywordInput, KeywordPrediction>? _engine;
    private ITransformer? _model;

    private readonly string _modelPath = Path.Combine("ml", "model.zip");

    private readonly HashSet<string> _usefulKeywords = new()
    {
        "age","height","wife","girlfriend","father","mother","brother","sister",
        "family","biography","bio","real name","net worth","birthday","birth date",
        "date of birth","bday","tattoo","wiki","wikipedia","film","films",
        "web series","instagram","insta","facebook","twitter","religion","caste"
    };

    public KeywordMLService()
    {
        _ml = new MLContext();

        // Try loading model on startup
        LoadModelFromDisk();
    }

    // --------------------------------------------------------------------
    // READ EXCEL & TRAINING PIPELINE
    // --------------------------------------------------------------------
    public string Train(string excelPath)
    {
        var data = ReadExcel(excelPath);

        var trainingRows = data.Select(x => new KeywordInput
        {
            Volume = x.Volume,
            TrendAvg = TrendHelper.ParseTrend(x.Trend),
            KeywordDifficulty = x.KeywordDifficulty,
            CompetitiveDensity = x.CompetitiveDensity,
            NumberOfResults = x.NumberOfResults
        });

        var trainingData = _ml.Data.LoadFromEnumerable(trainingRows);

        var pipeline = _ml.Transforms.Concatenate("Features",
                nameof(KeywordInput.Volume), nameof(KeywordInput.TrendAvg),
                nameof(KeywordInput.KeywordDifficulty), nameof(KeywordInput.CompetitiveDensity),
                nameof(KeywordInput.NumberOfResults))
            .Append(_ml.BinaryClassification.Trainers.LbfgsLogisticRegression());

        _model = pipeline.Fit(trainingData);

        _engine = _ml.Model.CreatePredictionEngine<KeywordInput, KeywordPrediction>(_model);

        SaveModelToDisk();

        return "Model trained and saved successfully.";
    }

    private List<KeywordData> ReadExcel(string filePath)
    {
        var list = new List<KeywordData>();

        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var rows = sheet.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            list.Add(new KeywordData
            {
                Keyword = row.Cell(1).GetValue<string>().Trim(),
                Intent = row.Cell(2).GetValue<string>().Trim(),
                Volume = float.TryParse(row.Cell(3).GetValue<string>(), out var v) ? v : 0,
                Trend = row.Cell(4).GetValue<string>().Trim(),
                KeywordDifficulty = float.TryParse(row.Cell(5).GetValue<string>(), out var kd) ? kd : 0,
                CompetitiveDensity = float.TryParse(row.Cell(6).GetValue<string>(), out var cd) ? cd : 0,
                SERPFeatures = row.Cell(7).GetValue<string>().Trim(),
                NumberOfResults = float.TryParse(row.Cell(8).GetValue<string>(), out var r) ? r : 0,
                Useful = _usefulKeywords.Contains(row.Cell(1).GetValue<string>().Trim())
            });
        }

        return list;
    }
    public List<KeywordResult> ExtractBioRelevantKeywords(string excelPath)
    {
        var results = new List<KeywordResult>();

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.First();
        var rows = sheet.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            string keyword = row.Cell(1).GetValue<string>().Trim();
            float volume = float.TryParse(row.Cell(3).GetValue<string>(), out var v) ? v : 0;

            // Check if keyword contains any bio-relevant keyword
            bool isBioRelevant = _usefulKeywords.Any(bio =>
                keyword.Contains(bio, StringComparison.OrdinalIgnoreCase));

            if (isBioRelevant)
            {
                results.Add(new KeywordResult(keyword, volume));
            }
        }

        return results;
    }

    public KeywordPrediction Predict(KeywordInput input)
    {
        if (_engine == null)
            throw new Exception("Model is not trained or loaded.");

        return _engine.Predict(input);
    }
    private void SaveModelToDisk()
    {
        Directory.CreateDirectory("ml");

        _ml.Model.Save(_model, inputSchema: null, filePath: _modelPath);
    }

    private void LoadModelFromDisk()
    {
        if (!File.Exists(_modelPath))
            return;

        _model = _ml.Model.Load(_modelPath, out var schema);
        _engine = _ml.Model.CreatePredictionEngine<KeywordInput, KeywordPrediction>(_model);
    }
}
