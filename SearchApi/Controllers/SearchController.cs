using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;

// Alias, um Namenskonflikte mit SearchApi.Elastic zu vermeiden
using Ecs = global::Elastic.Clients.Elasticsearch;

using SearchApi.Elastic;   
using SearchApi.Models;    
using SearchApi.Parsing;   

namespace SearchApi.Controllers;

using Ecs.QueryDsl;
using SearchApi.Parsing.Interfaces;

[ApiController]
[Route("api/[controller]")]
public sealed class SearchController(ElasticsearchService es) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Search([FromBody] Models.SearchRequest request)
    {
        if (String.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query must not be empty");
        
        INode ast = BoolParser.Parse(request.Query);
        Console.WriteLine("AST: " + AbstractSyntaxTreeDebug.Dump(ast));
        Query esQuery = EsQueryBuilder.ToEsQuery(ast);
        Int32 size = request.Size <= 0 ? 10 : request.Size;
        
        Ecs.SearchRequest req = new Ecs.SearchRequest
        {
            Size  = size,
            Query = esQuery,
        };
        
        if (!String.IsNullOrWhiteSpace(request.Index))
            req.Indices = request.Index;
        
        SearchResponse<Document> response = await es.Client.SearchAsync<Document>(req);

        if (!response.IsSuccess())
            return StatusCode(500, response.ElasticsearchServerError?.ToString() ?? "Search failed");

        return Ok(new
        {
            took = response.Took,
            hits = response.Hits.Select(h => new
            {
                id = h.Id,
                score = h.Score,
                source = h.Source,
            }),
        });
    }
}
