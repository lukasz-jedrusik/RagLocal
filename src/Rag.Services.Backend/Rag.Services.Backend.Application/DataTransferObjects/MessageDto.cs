namespace Rag.Services.Backend.Application.DataTransferObjects
{
    public class MessageDto
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<SourceDto> Sources { get; set; } = [];
    }
}
