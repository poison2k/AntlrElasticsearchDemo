using System.Text;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace SearchApi.UnitTests.TestUtils;

/// <summary> 
/// Captures the final JSON request body that the official Elasticsearch .NET client would send, 
/// without requiring a real Elasticsearch server. 
/// </summary> 
public static class EsSerialize
{
    /// <summary> 
    /// Executes an Elasticsearch client call against an in-memory transport and returns the JSON request body. 
    /// </summary> 
    public static async Task<String> CaptureRequestBodyAsync(Func<ElasticsearchClient, Task> call)
    {
        // Required by the client to consider the "server" a supported Elasticsearch distribution. 
        // See: x-elastic-product header requirement when using in-memory invokers. 
        Dictionary<String, IEnumerable<String>> headers = new()
        {
            {"x-elastic-product", ["Elasticsearch"]},
        };

        // Minimal valid JSON response. For most endpoints, we don't care about the response content, 
        // we only want the outgoing request bytes. 
        Byte[] responseBytes = "{}"u8.ToArray();
        InMemoryRequestInvoker invoker = new InMemoryRequestInvoker(responseBytes, headers: headers);
        Byte[]? capturedRequestBytes = null;
        ElasticsearchClientSettings settings = new ElasticsearchClientSettings(invoker)
            .EnableDebugMode(details =>
            {
                // EnableDebugMode turns on DisableDirectStreaming etc. so we can capture the request bytes. 2 
                capturedRequestBytes = details.RequestBodyInBytes;
            });

        ElasticsearchClient client = new(settings);

        await call(client);
        if (capturedRequestBytes is null || capturedRequestBytes.Length == 0)
            return String.Empty;

        return Encoding.UTF8.GetString(capturedRequestBytes);
    }

    /// <summary> 
    /// Convenience overload for Search: provides a realistic minimal search response JSON so the client can deserialize. 
    /// Use this if your call requires deserialization of a response. 
    /// </summary> 
    public static async Task<String> CaptureSearchRequestBodyAsync<TDocument>(Func<ElasticsearchClient, Task> call)
    {
        Dictionary<String, IEnumerable<String>> headers = new()
        {
            {"x-elastic-product", ["Elasticsearch"]},
        };

        // Minimal valid _search response (ES 8.x style total object). 
        const String searchResponseJson = """ 
                                          { 
                                            "took": 0, 
                                            "timed_out": false, 
                                            "_shards": { "total": 1, "successful": 1, "skipped": 0, "failed": 0 }, 
                                            "hits": { 
                                              "total": { "value": 0, "relation": "eq" }, 
                                              "max_score": null, 
                                              "hits": [] 
                                            } 
                                          } 
                                          """;

        Byte[] responseBytes = Encoding.UTF8.GetBytes(searchResponseJson);
        InMemoryRequestInvoker invoker = new(responseBytes, headers: headers);
        Byte[]? capturedRequestBytes = null;
        ElasticsearchClientSettings settings = new ElasticsearchClientSettings(invoker)
            .EnableDebugMode(details => capturedRequestBytes = details.RequestBodyInBytes);
        ElasticsearchClient client = new(settings);

        await call(client);

        if (capturedRequestBytes is null || capturedRequestBytes.Length == 0)
            return String.Empty;

        return Encoding.UTF8.GetString(capturedRequestBytes);
    }
}