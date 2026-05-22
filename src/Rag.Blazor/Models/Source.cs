namespace Rag.Blazor.Models;

public class Source
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public double Score { get; set; }
}
