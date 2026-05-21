namespace Rag.Blazor.Services
{
    public class ChatApiClient
    {
        private readonly HttpClient _http;

        public ChatApiClient(HttpClient http)
        {
            _http = http;
        }
    }
}