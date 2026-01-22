using Testcontainers.Elasticsearch;

namespace  SearchApi.IntegrationTests.Infrastructure;

using DotNet.Testcontainers.Images;

public sealed class ElasticsearchFixture
{
    public ElasticsearchContainer Container { get; }
    
    public ElasticsearchFixture()
    {
        
        DockerImage image = new("docker.elastic.co/elasticsearch/elasticsearch:9.2.4"); 
   
        Container = new ElasticsearchBuilder(image)
            .WithCleanUp(true)
            .Build();
    }

    public Task StartAsync() => Container.StartAsync();
    public Task StopAsync() => Container.DisposeAsync().AsTask();
}
