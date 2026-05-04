namespace DocumentAnalyzer.Models
{
    public class DocumentChunk
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public string Content { get; set; }

    public string Embedding { get; set; } // store as JSON string

    public Document Document { get; set; }
}
}
