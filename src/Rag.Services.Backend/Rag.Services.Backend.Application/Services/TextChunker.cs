namespace Rag.Services.Backend.Application.Services
{
    public static class TextChunker
    {
        public static List<string> Chunk(string text, int size = 500)
        {
            var chunks = new List<string>();

            for (int i = 0; i < text.Length; i += size)
            {
                var chunk = text.Substring(i, Math.Min(size, text.Length - i));
                chunks.Add(chunk);
            }
                
            return chunks;
        }
    }
}