namespace SearchApi.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;


public class TestWebApplicationFactory(String esUrl) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Elasticsearch:NodeUri", esUrl);
        builder.UseSetting("Elasticsearch:Username", "elastic");
        builder.UseSetting("Elasticsearch:Password", "changeme");
        builder.UseSetting("Elasticsearch:DefaultIndex", "documents");
        builder.UseEnvironment("Testing");
    }
}