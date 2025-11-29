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

        Directory.CreateDirectory("uploads");

        var savePath = Path.Combine("uploads", file.FileName);

        using (var stream = new FileStream(savePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var extractor = new BioKeywordExtractor(_ai);
        var output = await extractor.ExtractBioKeywordsAsync(savePath);

        return Ok(output);
    }
}
