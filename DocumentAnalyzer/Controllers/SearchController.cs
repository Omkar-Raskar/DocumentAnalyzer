using DocumentAnalyzer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using DocumentAnalyzer.Models;
using DocumentAnalyzer.Data;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;

    public SearchController(AppDbContext context, IEmbeddingService embeddingService)
    {
        _context = context;
        _embeddingService = embeddingService;
    }

    private int KeywordScore(string text, string query)
    {
        var queryWords = query.ToLower().Split(' ');
        int score = 0;

        foreach (var word in queryWords)
        {
            if (text.ToLower().Contains(word))
                score++;
        }

        return score;
    }

    [HttpPost("query")]
    public async Task<IActionResult> Search([FromBody] string query)
    {
        // Step 1: Convert query to embedding
        var queryEmbedding = await _embeddingService.GetEmbedding(query);

        // Step 2: Get all chunks from DB
        var chunks = _context.DocumentChunks.ToList();

        var results = new List<(string content, double score)>();

        foreach (var chunk in chunks)
        {
            var chunkEmbedding = JsonSerializer.Deserialize<float[]>(chunk.Embedding);

            var semanticScore = SimilarityHelper.CosineSimilarity(queryEmbedding, chunkEmbedding);
            var keywordScore = KeywordScore(chunk.Content, query);

            var finalScore = (0.7 * semanticScore) + (0.3 * keywordScore);

            results.Add((chunk.Content, finalScore));
        }

        // Top 5 results
        var topResults = results
            .OrderByDescending(x => x.score)
            .Take(5)
            .Select(x => new { x.content, x.score });
        return Ok(topResults);
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] string query)
    {
        // Step 1: Get query embedding
        var queryEmbedding = await _embeddingService.GetEmbedding(query);

        var chunks = _context.DocumentChunks.ToList();

        //var results = new List<(string content, double score)>();

        //foreach (var chunk in chunks)
        //{
        //    var chunkEmbedding = JsonSerializer.Deserialize<float[]>(chunk.Embedding);

        //    var similarity = SimilarityHelper.CosineSimilarity(queryEmbedding, chunkEmbedding);

        //    results.Add((chunk.Content, similarity));
        //}

        var results = new List<(string content, double score)>();

        foreach (var chunk in chunks)
        {
            var chunkEmbedding = JsonSerializer.Deserialize<float[]>(chunk.Embedding);

            var semanticScore = SimilarityHelper.CosineSimilarity(queryEmbedding, chunkEmbedding);
            var keywordScore = KeywordScore(chunk.Content, query);

            var finalScore = (0.7 * semanticScore) + (0.3 * keywordScore);

            results.Add((chunk.Content, finalScore));
        }
        // Step 2: Get top chunks
        var topChunks = results
    .OrderByDescending(x => x.score)
    .Take(7)
    .Select(x => x.content)
    .Distinct()
    .ToList();

        // Step 3: Build prompt
        var contextText = string.Join("\n\n", topChunks);

        var prompt = $@"
You are an AI assistant. Answer based ONLY on the context below.Do NOT add any external knowledge.If the answer is not present, say 'Not available'.Do NOT add any information not present in the context.Do not omit any important detail mentioned in the context.

Context:
{contextText}

Question:
{query}
";

        // Step 4: Generate answer
        //var llmService = HttpContext.RequestServices.GetRequiredService<ILLMService>();
        var factory = HttpContext.RequestServices.GetRequiredService<LLMServiceFactory>();
        var llmService = factory.GetService();

        var answer = await llmService.GenerateResponse(prompt);

        return Ok(new
        {
            answer = answer,
            sources = topChunks
        });
    }
}