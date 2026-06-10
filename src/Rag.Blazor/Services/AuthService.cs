using System.Text.Json;
using Microsoft.JSInterop;
using Rag.Blazor.Models;

namespace Rag.Blazor.Services;

public class AuthService
{
    private readonly IJSRuntime _jsRuntime;
    private UserInfo? _currentUser;

    public event Action? OnAuthStateChanged;

    public AuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public UserInfo? CurrentUser => _currentUser;

    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentUser.IdToken);

    public string? GetToken() => _currentUser?.IdToken;

    public async Task InitializeAsync()
    {
        try
        {
            // Sprawdź czy użytkownik jest już zalogowany (token w sessionStorage)
            var userJson = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "user");
            if (!string.IsNullOrEmpty(userJson))
            {
                _currentUser = JsonSerializer.Deserialize<UserInfo>(userJson);
                NotifyAuthStateChanged();
            }
        }
        catch
        {
            // Ignore errors during initialization
        }
    }

    public async Task SignInAsync(UserInfo userInfo)
    {
        _currentUser = userInfo;

        // Zapisz token w sessionStorage
        var userJson = JsonSerializer.Serialize(userInfo);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "user", userJson);

        NotifyAuthStateChanged();
    }

    public async Task SignOutAsync()
    {
        _currentUser = null;

        // Usuń token z sessionStorage
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "user");
        await _jsRuntime.InvokeVoidAsync("googleSignOut");

        NotifyAuthStateChanged();
    }

    private void NotifyAuthStateChanged()
    {
        OnAuthStateChanged?.Invoke();
    }
}
