namespace SearchApi.IntegrationTests.Models;

using System.Text.Json.Serialization;

public class SearchResponseDto
{
    [JsonPropertyName("hits")] 
    public List<SearchHitDto> Hits { get; init; } = [];
}

// ReSharper disable once ClassNeverInstantiated.Global
// Justification = Instantiated by System.Text.Json during deserialize
public sealed record SearchHitDto
{
    [JsonPropertyName("id")] 
    public String Id { get; set; } = String.Empty;
}
    
    
