namespace DocumentAnalyzer.Models
{
    public class Document
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string Content { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }
    }
}
