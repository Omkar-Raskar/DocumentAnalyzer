using DocumentAnalyzer.Data;
using DocumentAnalyzer.Models;
using DocumentAnalyzer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pgvector;
using System.Security.Claims;


[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TextExtractionService _textService;

    public DocumentController(AppDbContext context, TextExtractionService textService)
    {
        _context = context;
        _textService = textService;
    }


    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file");

        string content = "";

        using (var stream = file.OpenReadStream())
        {
            if (file.FileName.EndsWith(".pdf"))
            {
                content = _textService.ExtractTextFromPdf(stream);
            }
            else if (file.FileName.EndsWith(".docx"))
            {
                content = _textService.ExtractTextFromDocx(stream);
            }
            else
            {
                return BadRequest("Unsupported file type");
            }
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var document = new Document
        {
            FileName = file.FileName,
            Content = content,
            UserId = userId
        };

        // ✅ SAVE DOCUMENT FIRST
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // 🔥 ADD THIS PART HERE
        var chunkService = HttpContext.RequestServices.GetRequiredService<ChunkingService>();
        var factory =
    HttpContext.RequestServices
        .GetRequiredService<EmbeddingServiceFactory>();

        var embeddingService =
            factory.GetService();
        //var embeddingService = HttpContext.RequestServices.GetRequiredService<IEmbeddingService>();

        var chunks = chunkService.SplitText(content, 300, 75);

        foreach (var chunk in chunks)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("NEW CHUNK");

            if (string.IsNullOrWhiteSpace(chunk))
            {
                Console.WriteLine("Chunk is empty");
                continue;
            }

            Console.WriteLine($"Chunk Length: {chunk.Length}");

            var embedding = await embeddingService.GetEmbedding(chunk);

            if (embedding == null)
            {
                Console.WriteLine("Embedding is NULL");
                continue;
            }

            //Console.WriteLine($"Embedding Count: {embedding.Count}");

            //if (embedding.Count == 0)
            //{
            //    Console.WriteLine("Embedding is EMPTY");
            //    continue;
            //}

            var chunkEntity = new DocumentChunk
            {
                DocumentId = document.Id,
                Content = chunk,
                Embedding = new Vector(embedding.ToArray())
            };

            _context.DocumentChunks.Add(chunkEntity);

            Console.WriteLine("Chunk Added Successfully");
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "File uploaded, chunked, and embedded successfully" });
    }

    // Embedding = System.Text.Json.JsonSerializer.Serialize(embedding)

    //[Authorize]
    //[HttpPost("upload")]
    //public async Task<IActionResult> Upload(IFormFile file)
    //{
    //    if (file == null || file.Length == 0)
    //        return BadRequest("Invalid file");

    //    string content = "";

    //    using (var stream = file.OpenReadStream())
    //    {
    //        if (file.FileName.EndsWith(".pdf"))
    //        {
    //            content = _textService.ExtractTextFromPdf(stream);
    //        }
    //        else if (file.FileName.EndsWith(".docx"))
    //        {
    //            content = _textService.ExtractTextFromDocx(stream);
    //        }
    //        else
    //        {
    //            return BadRequest("Unsupported file type");
    //        }
    //    }

    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    var document = new Document
    //    {
    //        FileName = file.FileName,
    //        Content = content,
    //        UserId = userId
    //    };

    //    _context.Documents.Add(document);
    //    await _context.SaveChangesAsync();

    //    return Ok(new { message = "File uploaded and processed" });
    //}
}