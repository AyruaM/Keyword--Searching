using System.Net.Http.Json;

public class OllamaChatService
{
    private readonly HttpClient _http;

    public OllamaChatService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ollama");
    }

    public async Task<string> ChatAsync(List<Dictionary<string, string>> messages)
    {
        var request = new
        {
            model = "deepseek-r1:1.5b",
            messages = messages,
            stream = false
        };

        var response = await _http.PostAsJsonAsync("/api/chat", request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

        return json?.Message?.Content ?? "(no response)";
    }
}

public class OllamaChatResponse
{
    public OllamaMessage? Message { get; set; }
}

public class OllamaMessage
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}
