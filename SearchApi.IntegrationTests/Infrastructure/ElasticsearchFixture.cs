using Testcontainers.Elasticsearch;

namespace  SearchApi.IntegrationTests.Infrastructure;

using DotNet.Testcontainers.Images;

public sealed class ElasticsearchFixture
{
    public ElasticsearchContainer Container { get; }
    
    public ElasticsearchFixture()
    {
        
        DockerImage image = new("cr.rt.coi.air:443/elastic/elasticsearch:9.1.0"); 
   
        Container = new ElasticsearchBuilder(image)
            .WithCleanUp(true)
            .Build();
    }

    public Task StartAsync() => Container.StartAsync();
    public Task StopAsync() => Container.DisposeAsync().AsTask();
}