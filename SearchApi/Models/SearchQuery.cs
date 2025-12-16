namespace SearchApi.Models;


public sealed class SearchRequest
{
    public string? Index { get; set; }
    public string Query { get; set; } = string.Empty; // z.B. "TEST AND CHILD Or Good"
    public int Size { get; set; } = 10;
}