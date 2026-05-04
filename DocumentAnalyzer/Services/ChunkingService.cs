namespace DocumentAnalyzer.Services
{
    public class ChunkingService
    {
        public List<string> SplitText(string text, int chunkSize = 300, int overlap = 75)
        {
            var words = text.Split(' ');
            var chunks = new List<string>();

            for (int i = 0; i < words.Length; i += (chunkSize - overlap))
            {
                var chunkWords = words.Skip(i).Take(chunkSize);
                var chunk = string.Join(" ", chunkWords);

                if (!string.IsNullOrWhiteSpace(chunk) && chunk.Length > 100)
                {
                    chunks.Add(chunk);
                }
            }

            return chunks;
        }

        //public List<string> SplitText(string text, int chunkSize = 500)
        //{
        //    var chunks = new List<string>();

        //    for (int i = 0; i < text.Length; i += chunkSize)
        //    {
        //        chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
        //    }

        //    return chunks;
        //}
    }
}
