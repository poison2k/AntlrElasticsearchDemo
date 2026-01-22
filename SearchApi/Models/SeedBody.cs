namespace SearchApi.Models;

public sealed class SeedBody(String? index)
{
    public String? Index { get; } = index;
    public Document[] Docs { get; init; } = [];
}