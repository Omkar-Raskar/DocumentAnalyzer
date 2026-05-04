using DocumentAnalyzer.Data;
using DocumentAnalyzer.Models;
using DocumentAnalyzer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var embeddingService = HttpContext.RequestServices.GetRequiredService<IEmbeddingService>();

        var chunks = chunkService.SplitText(content, 300, 75);

        foreach (var chunk in chunks)
        {
            var embedding = await embeddingService.GetEmbedding(chunk);

            var chunkEntity = new DocumentChunk
            {
                DocumentId = document.Id,
                Content = chunk,
                Embedding = System.Text.Json.JsonSerializer.Serialize(embedding)
            };

            _context.DocumentChunks.Add(chunkEntity);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "File uploaded, chunked, and embedded successfully" });
    }

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