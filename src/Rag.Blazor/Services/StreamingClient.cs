using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Rag.Blazor.Models;

namespace Rag.Blazor.Services;

public class StreamingClient
{
    private readonly HttpClient _http;
    private readonly ApiSettings _apiSettings;

    public StreamingClient(HttpClient http, ApiSettings apiSettings)
    {
        _http = http;
        _apiSettings = apiSettings;
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
            _apiSettings.GetStreamUrl())
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

        string? currentEvent = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);

            if (line == null)
                break;

            if (string.IsNullOrWhiteSpace(line))
            {
                currentEvent = null;
                continue;
            }

            // Parse event type
            if (line.StartsWith("event:"))
            {
                currentEvent = line["event:".Length..].Trim();
                continue;
            }

            // Parse data
            if (!line.StartsWith("data:"))
                continue;

            var data = line["data:".Length..].Trim();

            // Check for [DONE] marker
            if (data == "[DONE]")
            {
                yield return new StreamChunk
                {
                    IsCompleted = true
                };
                break;
            }

            // Handle different event types
            if (currentEvent == "meta")
            {
                var meta = JsonSerializer.Deserialize<MetaData>(data);
                if (meta?.ConversationId != null)
                {
                    yield return new StreamChunk
                    {
                        ConversationId = meta.ConversationId,
                        IsConversationId = true
                    };
                }
            }
            else if (currentEvent == "citations")
            {
                var citations = JsonSerializer.Deserialize<CitationsData>(data);
                if (citations?.Sources != null)
                {
                    yield return new StreamChunk
                    {
                        IsCitations = true,
                        Sources = citations.Sources
                    };
                }
            }
            else
            {
                // Regular token chunk
                var tokenData = JsonSerializer.Deserialize<TokenData>(data);
                if (tokenData?.Token != null)
                {
                    yield return new StreamChunk
                    {
                        Content = tokenData.Token
                    };
                }
            }
        }
    }

    private sealed class MetaData
    {
        public Guid ConversationId { get; init; }
    }

    private sealed class TokenData
    {
        public string Token { get; init; } = string.Empty;
    }

    private sealed class CitationsData
    {
        public List<Source>? Sources { get; init; }
    }
}