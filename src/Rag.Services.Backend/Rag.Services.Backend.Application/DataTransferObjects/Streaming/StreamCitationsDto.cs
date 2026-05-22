namespace Rag.Services.Backend.Application.DataTransferObjects.Streaming
{
    public class StreamCitationsDto
    {
        public List<StreamSourceDto> Sources { get; set; } = new();
    }
}
