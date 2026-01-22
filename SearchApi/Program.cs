using SearchApi.Elastic;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ElasticsearchConfig>(builder.Configuration.GetSection("Elasticsearch"));
builder.Services.AddSingleton<ElasticsearchService>();
builder.Services.AddControllers();

WebApplication app = builder.Build();
app.MapControllers();
await app.RunAsync();

public abstract partial class Program {}