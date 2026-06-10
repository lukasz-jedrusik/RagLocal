using System.Net.Http.Headers;

namespace Rag.Blazor.Services;

public class AuthenticatedHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public AuthenticatedHttpClient(AuthService authService)
    {
        _httpClient = new HttpClient();
        _authService = authService;

        // Nasłuchuj zmian w stanie autentykacji
        _authService.OnAuthStateChanged += UpdateAuthorizationHeader;

        // Ustaw początkowy token
        UpdateAuthorizationHeader();
    }

    public HttpClient HttpClient => _httpClient;

    private void UpdateAuthorizationHeader()
    {
        var token = _authService.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
