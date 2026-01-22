using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using EsIdx = Elastic.Clients.Elasticsearch.IndexManagement;
using Microsoft.Extensions.Options;
using Elastic.Transport;
using SearchApi.Models;

namespace SearchApi.Elastic;

using System.Text;

public sealed class ElasticsearchService
{
    public ElasticsearchService(IOptions<ElasticsearchConfig> options)
    {
        ElasticsearchConfig cfg = options.Value;
        String defaultIndex = cfg.DefaultIndex;

        Uri node;
        if (!String.IsNullOrWhiteSpace(cfg.CloudId))
        {
            node = new Uri(cfg.CloudId);
        }
        else if (!String.IsNullOrWhiteSpace(cfg.NodeUri))
        {
            node = new Uri(cfg.NodeUri);
        }
        else
        {
            node = new Uri($"https://localhost:9200");
        }
        
        ElasticsearchClientSettings settings = new ElasticsearchClientSettings(node)
            .Authentication(new BasicAuthentication(cfg.Username, cfg.Password))
            .DefaultIndex(defaultIndex)
            .ServerCertificateValidationCallback((_, _, _, _) => true) 
            .EnableDebugMode(details =>
            {
                if (details.RequestBodyInBytes == null) return;
                Console.WriteLine("=== ES Request ===");
                Console.WriteLine(Encoding.UTF8.GetString(details.RequestBodyInBytes));
            }); 

        Client = new ElasticsearchClient(settings);
    }

    public ElasticsearchClient Client { get; }

    public async Task<Boolean> EnsureIndexAsync(String indexName)
    {
        EsIdx.ExistsResponse exists = await Client.Indices.ExistsAsync(indexName);
        if (exists.Exists) return true;

        EsIdx.CreateIndexRequest req = new (indexName)
        {
            Settings = new IndexSettings
            {
                NumberOfShards = 1,
                NumberOfReplicas = 0,
            },
        };

        CreateIndexResponse create = await Client.Indices.CreateAsync(req);
        return create.IsSuccess();
    }

    public async Task IndexManyAsync(String indexName, IEnumerable<Document> docs)
    {
        BulkRequest bulk = new (indexName)
        {
            Refresh = Refresh.WaitFor,
            Operations = new List<IBulkOperation>(),
        };

        foreach (Document d in docs)
            bulk.Operations.Add(new BulkIndexOperation<Document>(d) { Id = d.Id });

        BulkResponse response = await Client.BulkAsync(bulk);

        if (!response.IsSuccess())
            throw new InvalidOperationException(response.ElasticsearchServerError?.ToString() ?? "Bulk failed");
    }
}
