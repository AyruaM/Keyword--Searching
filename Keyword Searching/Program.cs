var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("ollama", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434");
});

builder.Services.AddSingleton<OllamaChatService>();
builder.Services.AddSingleton<OllamaBioService>();
builder.Services.AddSingleton<BioKeywordExtractor>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Logging middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"➡️ Request: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"⬅️ Status: {context.Response.StatusCode}");
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
