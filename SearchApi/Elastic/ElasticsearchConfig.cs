namespace SearchApi.Elastic;

public sealed class ElasticsearchConfig
{
    public string? CloudId { get; set; }
    public string? NodeUri { get; set; }
    public string Username { get; set; } = "elastic";
    public string Password { get; set; } = "changeme";
    public string DefaultIndex { get; set; } = "documents";
}