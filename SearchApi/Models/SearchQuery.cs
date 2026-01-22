namespace SearchApi.Models;


public sealed class SearchRequest
{
    public String? Index { get; set; }
    public String Query { get; set; } = String.Empty; // z.B. "TEST AND CHILD Or Good"
    public Int32 Size { get; set; } = 100;
}