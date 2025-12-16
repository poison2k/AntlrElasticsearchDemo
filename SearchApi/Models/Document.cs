namespace SearchApi.Models;
public sealed class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Content { get; set; } = string.Empty; // Volltext
    public string[] Tags { get; set; } = Array.Empty<string>();
}