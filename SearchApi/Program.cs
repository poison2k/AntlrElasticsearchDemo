using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using SearchApi.Elastic;


var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<ElasticsearchConfig>(builder.Configuration.GetSection("Elasticsearch"));


builder.Services.AddSingleton<ElasticsearchService>();
builder.Services.AddControllers();


var app = builder.Build();
app.MapControllers();
app.Run();