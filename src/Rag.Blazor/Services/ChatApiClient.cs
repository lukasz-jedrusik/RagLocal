using System.Net.Http.Json;
using Rag.Blazor.Models;

namespace Rag.Blazor.Services
{
    public class ChatApiClient
    {
        private readonly HttpClient _http;
        private readonly ApiSettings _apiSettings;

        public ChatApiClient(HttpClient http, ApiSettings apiSettings)
        {
            _http = http;
            _apiSettings = apiSettings;
        }

        // Konwersacje

        public async Task<List<ConversationDto>> GetConversationsAsync()
        {
            var response = await _http.GetAsync($"{_apiSettings.BaseUrl}/conversations");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ConversationDto>>() ?? new();
        }

        public async Task<ConversationDetailDto?> GetConversationDetailAsync(Guid conversationId)
        {
            var url = $"{_apiSettings.BaseUrl}/conversations/{conversationId}";
            Console.WriteLine($"Fetching conversation from: {url}");

            var response = await _http.GetAsync(url);

            Console.WriteLine($"Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch conversation: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {content}");

            var result = await response.Content.ReadFromJsonAsync<ConversationDetailDto>();
            Console.WriteLine($"Parsed result - Messages count: {result?.Messages?.Count ?? 0}");

            return result;
        }

        public async Task<ConversationDto?> CreateConversationAsync(string title)
        {
            var request = new CreateConversationDto { Title = title };
            var response = await _http.PostAsJsonAsync($"{_apiSettings.BaseUrl}/conversations", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ConversationDto>();
        }

        public async Task<bool> DeleteConversationAsync(Guid conversationId)
        {
            var response = await _http.DeleteAsync($"{_apiSettings.BaseUrl}/conversations/{conversationId}");
            return response.IsSuccessStatusCode;
        }
    }
}