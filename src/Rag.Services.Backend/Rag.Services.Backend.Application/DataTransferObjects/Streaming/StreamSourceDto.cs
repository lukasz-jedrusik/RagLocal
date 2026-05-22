namespace Rag.Services.Backend.Application.DataTransferObjects.Streaming
{
    public class StreamSourceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public float Score { get; set; }
    }
}
