using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Rag.Blazor.Models;

namespace Rag.Blazor.Services;

public class StreamingClient
{
    private readonly HttpClient _http;

    public StreamingClient(HttpClient http)
    {
        _http = http;
    }

    public async IAsyncEnumerable<StreamChunk> StreamAsync(
        string question,
        Guid? conversationId,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            question,
            conversationId
        };

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://localhost:5001/ask/stream")
        {
            Content = JsonContent.Create(request)
        };

        var response = await _http.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        using var reader = new StreamReader(stream);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);

            if (line == null)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!line.StartsWith("data:"))
                continue;

            var content = line["data:".Length..].Trim();

            // conversation id
            if (content.StartsWith("[CONVERSATION_ID]"))
            {
                var idText = content.Replace(
                    "[CONVERSATION_ID]",
                    "");

                if (Guid.TryParse(idText, out var guid))
                {
                    yield return new StreamChunk
                    {
                        ConversationId = guid,
                        IsConversationId = true
                    };
                }

                continue;
            }

            // token
            yield return new StreamChunk
            {
                Content = content
            };
        }
    }
}