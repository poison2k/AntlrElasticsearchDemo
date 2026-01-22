namespace SearchApi.IntegrationTests.Hooks;

using Reqnroll;
using SearchApi.IntegrationTests.Infrastructure;

[Binding]
public class TestRunHooks
{
    private static ElasticsearchFixture? _es;
    private static TestWebApplicationFactory? _factory;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        _es = new ElasticsearchFixture();
        await _es.StartAsync();

        _factory = new TestWebApplicationFactory(_es.Container.GetConnectionString());
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        if (_es is not null)
            await _es.StopAsync();
    }

    [BeforeScenario]
    public void BeforeScenario(ScenarioContext ctx)
    {
        HttpClient http = _factory!.CreateClient();
        ctx.Set(http);
    }
    
}