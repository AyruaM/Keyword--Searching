using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/bio")]
public class BioKeywordsAIController : ControllerBase
{
    private readonly OllamaBioService _ai;

    public BioKeywordsAIController(OllamaBioService ai)
    {
        _ai = ai;
    }

    [HttpPost("bio-keywords-ai")]
    public async Task<IActionResult> GetBioKeywordsAI(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Input folder
        string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploadFolder);

        // Save uploaded file
        string inputPath = Path.Combine(uploadFolder, file.FileName);
        using (var stream = new FileStream(inputPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var extractor = new BioKeywordExtractor(_ai);

        // 1️⃣ Extract result list
        var results = await extractor.ExtractBioKeywordsAsync(inputPath);

        // 2️⃣ Save results to Excel
        string outputFilePath = extractor.SaveResultsToExcel(results, uploadFolder);

        // 3️⃣ Return as file download
        var bytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);
        string fileName = Path.GetFileName(outputFilePath);

        return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
    }


}
