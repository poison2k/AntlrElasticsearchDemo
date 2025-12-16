using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using EsIdx = Elastic.Clients.Elasticsearch.IndexManagement;
using Microsoft.Extensions.Options;
using Elastic.Transport;
using SearchApi.Models;



namespace SearchApi.Elastic;

public sealed class ElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _defaultIndex;

    public ElasticsearchService(IOptions<ElasticsearchConfig> options)
    {
        var cfg = options.Value;
        _defaultIndex = cfg.DefaultIndex;

        ElasticsearchClientSettings settings;

        if (!string.IsNullOrWhiteSpace(cfg.CloudId))
        {
            var node = new Uri(string.IsNullOrWhiteSpace(cfg.NodeUri)
                ? "https://localhost:9200"
                : cfg.NodeUri);

            settings = new ElasticsearchClientSettings(node)
                .Authentication(new BasicAuthentication(cfg.Username, cfg.Password))
                .DefaultIndex(_defaultIndex)
                .ServerCertificateValidationCallback((_, _, _, _) => true); // nur dev

        }
        else
        {
            var node = new Uri(string.IsNullOrWhiteSpace(cfg.NodeUri)
                ? "https://localhost:9200"
                : cfg.NodeUri);                  // ✅ String → Uri

            settings = new ElasticsearchClientSettings(node)
                .Authentication(new BasicAuthentication(cfg.Username, cfg.Password))
                .DefaultIndex(_defaultIndex)
                .ServerCertificateValidationCallback((_, _, _, _) => true); // nur dev
        }

        _client = new ElasticsearchClient(settings);
    }


    public ElasticsearchClient Client => _client;

    public async Task<bool> EnsureIndexAsync(string indexName)
    {
        var exists = await _client.Indices.ExistsAsync(indexName);
        if (exists.Exists) return true;

        var req = new EsIdx.CreateIndexRequest(indexName)
        {
            Settings = new IndexSettings
            {
                NumberOfShards = 1,
                NumberOfReplicas = 0
            }
            // Mappings lassen wir für den Moment weg
        };

        var create = await _client.Indices.CreateAsync(req);
        return create.IsSuccess();
    }


    public async Task IndexManyAsync(string indexName, IEnumerable<Document> docs)
    {
        var bulk = new BulkRequest(indexName)
        {
            Refresh = Refresh.WaitFor,
            Operations = new List<IBulkOperation>()
        };

        foreach (var d in docs)
            bulk.Operations.Add(new BulkIndexOperation<Document>(d) { Id = d.Id });

        var response = await _client.BulkAsync(bulk);

        if (!response.IsSuccess())
            throw new InvalidOperationException(response.ElasticsearchServerError?.ToString() ?? "Bulk failed");
    }

}
