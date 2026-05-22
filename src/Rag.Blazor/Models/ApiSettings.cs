namespace Rag.Blazor.Models;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string StreamEndpoint { get; set; } = string.Empty;
    
    public string GetStreamUrl() => $"{BaseUrl}{StreamEndpoint}";
}
