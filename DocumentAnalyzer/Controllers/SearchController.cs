using DocumentAnalyzer.Services;
using Microsoft.AspNetCore.Mvc;
using DocumentAnalyzer.Models;
using DocumentAnalyzer.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        var queryVector = new Vector(queryEmbedding);

        // Step 2: Get all chunks from DB
        // Step 2: Get top semantic chunks from PostgreSQL
        var semanticChunks = await _context.DocumentChunks
            .Select(chunk => new
            {
                content = chunk.Content,
                semanticScore = 1 - chunk.Embedding.CosineDistance(queryVector)
            })
            .OrderByDescending(x => x.semanticScore)
            .Take(20)
            .ToListAsync();


        // Step 3: Apply hybrid scoring in C#
        var topResults = semanticChunks
            .Select(x => new
            {
                content = x.content,

                score =
                    (x.semanticScore * 0.7)
                    +
                    (KeywordScore(x.content, query) * 0.3)
            })
            .OrderByDescending(x => x.score)
            .Take(5)
            .ToList();

        return Ok(topResults);
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] string query)
    {
        // Step 1: Get query embedding
        var queryEmbedding = await _embeddingService.GetEmbedding(query);
        var queryVector = new Vector(queryEmbedding);

        // Step 2: Retrieve top semantic chunks
        var semanticChunks = await _context.DocumentChunks
    .Where(chunk =>
        chunk.Embedding != null)
    .Select(chunk => new
    {
        content = chunk.Content,
        semanticScore =
            1 - chunk.Embedding.CosineDistance(queryVector)
    })
    .OrderByDescending(x => x.semanticScore)
    .Take(20)
    .ToListAsync();


        // Step 3: Apply hybrid reranking in C#
        var retrievedChunks = semanticChunks
            .Select(x => new
            {
                content = x.content,

                score =
                    (x.semanticScore * 0.7)
                    +
                    (KeywordScore(x.content, query) * 0.3)
            })
            .OrderByDescending(x => x.score)
            .Take(10)
            .ToList();

        var topChunks = retrievedChunks
    .Where(x => x.score > 0.55)
    .Select(x => x.content)
    .Distinct()
    .Take(7)
    .ToList();
        // Step 3: Build prompt
        var contextText = string.Join("\n\n", topChunks);

        if (!topChunks.Any())
        {
            return Ok(new
            {
                answer = "No relevant information found in the document.",
                sources = new List<string>()
            });
        }

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

//using DocumentAnalyzer.Services;
//using Microsoft.AspNetCore.Mvc;
//using System.Text.Json;
//using DocumentAnalyzer.Models;
//using DocumentAnalyzer.Data;

//[ApiController]
//[Route("api/[controller]")]
//public class SearchController : ControllerBase
//{
//    private readonly AppDbContext _context;
//    private readonly IEmbeddingService _embeddingService;

//    public SearchController(AppDbContext context, IEmbeddingService embeddingService)
//    {
//        _context = context;
//        _embeddingService = embeddingService;
//    }

//    private int KeywordScore(string text, string query)
//    {
//        var queryWords = query.ToLower().Split(' ');
//        int score = 0;

//        foreach (var word in queryWords)
//        {
//            if (text.ToLower().Contains(word))
//                score++;
//        }

//        return score;
//    }

//    [HttpPost("query")]
//    public async Task<IActionResult> Search([FromBody] string query)
//    {
//        // Step 1: Convert query to embedding
//        var queryEmbedding = await _embeddingService.GetEmbedding(query);

//        // Step 2: Get all chunks from DB
//        var chunks = _context.DocumentChunks.ToList();

//        var results = new List<(string content, double score)>();
//        foreach (var chunk in chunks)
//        {
//            var chunkEmbedding = chunk.Embedding.ToArray();

//            var semanticScore = SimilarityHelper.CosineSimilarity(
//                queryEmbedding,
//                chunkEmbedding
//            );

//            var keywordScore = KeywordScore(chunk.Content, query);

//            var finalScore = (0.7 * semanticScore) + (0.3 * keywordScore);

//            results.Add((chunk.Content, finalScore));
//        }

//        //this is changed from working code
//        //foreach (var chunk in chunks)
//        //{
//        //    var chunkEmbedding = JsonSerializer.Deserialize<float[]>(chunk.Embedding);

//        //    var semanticScore = SimilarityHelper.CosineSimilarity(queryEmbedding, chunkEmbedding);
//        //    var keywordScore = KeywordScore(chunk.Content, query);

//        //    var finalScore = (0.7 * semanticScore) + (0.3 * keywordScore);

//        //    results.Add((chunk.Content, finalScore));
//        //}
//        //upto this

//        // Top 5 results
//        var topResults = results
//            .OrderByDescending(x => x.score)
//            .Take(5)
//            .Select(x => new { x.content, x.score });
//        return Ok(topResults);
//    }

//    [HttpPost("ask")]
//    public async Task<IActionResult> Ask([FromBody] string query)
//    {
//        // Step 1: Get query embedding
//        var queryEmbedding = await _embeddingService.GetEmbedding(query);

//        var chunks = _context.DocumentChunks.ToList();



//        var results = new List<(string content, double score)>();

//        foreach (var chunk in chunks)
//        {
//            var chunkEmbedding = chunk.Embedding.ToArray();

//            var semanticScore = SimilarityHelper.CosineSimilarity(
//                queryEmbedding,
//                chunkEmbedding
//            );

//            var keywordScore = KeywordScore(chunk.Content, query);

//            var finalScore = (0.7 * semanticScore) + (0.3 * keywordScore);

//            results.Add((chunk.Content, finalScore));
//        }
//        //this is changed from working code
//        //foreach (var chunk in chunks)
//        //{
//        //    var chunkEmbedding = JsonSerializer.Deserialize<float[]>(chunk.Embedding);

//        //    var semanticScore = SimilarityHelper.CosineSimilarity(queryEmbedding, chunkEmbedding);
//        //    var keywordScore = KeywordScore(chunk.Content, query);

//        //    var finalScore = (0.7 * semanticScore) + (0.3 * keywordScore);

//        //    results.Add((chunk.Content, finalScore));
//        //}

//        //upto this
//        // Step 2: Get top chunks
//        var topChunks = results
//    .OrderByDescending(x => x.score)
//    .Take(7)
//    .Select(x => x.content)
//    .Distinct()
//    .ToList();

//        // Step 3: Build prompt
//        var contextText = string.Join("\n\n", topChunks);

//        var prompt = $@"
//You are an AI assistant. Answer based ONLY on the context below.Do NOT add any external knowledge.If the answer is not present, say 'Not available'.Do NOT add any information not present in the context.Do not omit any important detail mentioned in the context.

//Context:
//{contextText}

//Question:
//{query}
//";

//        // Step 4: Generate answer
//        //var llmService = HttpContext.RequestServices.GetRequiredService<ILLMService>();
//        var factory = HttpContext.RequestServices.GetRequiredService<LLMServiceFactory>();
//        var llmService = factory.GetService();

//        var answer = await llmService.GenerateResponse(prompt);

//        return Ok(new
//        {
//            answer = answer,
//            sources = topChunks
//        });
//    }
//}