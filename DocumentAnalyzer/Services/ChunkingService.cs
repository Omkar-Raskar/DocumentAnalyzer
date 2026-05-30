using System.Text.RegularExpressions;

namespace DocumentAnalyzer.Services
{
    public class ChunkingService
    {
        public List<string> SplitText(
            string text,
            int chunkSize = 800,
            int overlap = 150)
        {
            // Clean text
            text = text.Replace("\r", " ")
                       .Replace("\n", " ")
                       .Trim();

            // Split into sentences
            var sentences = Regex.Split(
                text,
                @"(?<=[.!?])\s+");

            var chunks = new List<string>();

            var currentChunk = "";
            int currentLength = 0;

            foreach (var sentence in sentences)
            {
                // Skip tiny/noisy sentences
                if (sentence.Trim().Length < 20)
                    continue;

                // If chunk exceeds limit → save current chunk
                if (currentLength + sentence.Length > chunkSize)
                {
                    if (!string.IsNullOrWhiteSpace(currentChunk))
                    {
                        chunks.Add(currentChunk.Trim());
                    }

                    // Add overlap from previous chunk
                    currentChunk = GetOverlapText(
                        currentChunk,
                        overlap);

                    currentLength = currentChunk.Length;
                }

                currentChunk += " " + sentence;
                currentLength += sentence.Length;
            }

            // Add last chunk
            if (!string.IsNullOrWhiteSpace(currentChunk))
            {
                chunks.Add(currentChunk.Trim());
            }

            return chunks;
        }

        private string GetOverlapText(
            string text,
            int overlapSize)
        {
            if (text.Length <= overlapSize)
                return text;

            return text.Substring(
                text.Length - overlapSize);
        }
    }
}

//namespace DocumentAnalyzer.Services
//{
//    public class ChunkingService
//    {
//        public List<string> SplitText(string text, int chunkSize = 300, int overlap = 75)
//        {
//            var words = text.Split(' ');
//            var chunks = new List<string>();

//            for (int i = 0; i < words.Length; i += (chunkSize - overlap))
//            {
//                var chunkWords = words.Skip(i).Take(chunkSize);
//                var chunk = string.Join(" ", chunkWords);

//                if (!string.IsNullOrWhiteSpace(chunk) && chunk.Length > 100)
//                {
//                    chunks.Add(chunk);
//                }
//            }

//            return chunks;
//        }

//        //public List<string> SplitText(string text, int chunkSize = 500)
//        //{
//        //    var chunks = new List<string>();

//        //    for (int i = 0; i < text.Length; i += chunkSize)
//        //    {
//        //        chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
//        //    }

//        //    return chunks;
//        //}
//    }
//}
