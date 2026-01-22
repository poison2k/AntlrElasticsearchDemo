namespace SearchApi.Elastic;

public sealed class ElasticsearchConfig
{
    public String? CloudId { get; set; }
    public String? NodeUri { get; set; }
    public String Username { get; set; } = "elastic";
    public String Password { get; set; } = "changeme";
    public String DefaultIndex { get; set; } = "documents";
}