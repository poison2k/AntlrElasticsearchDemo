using System.Linq;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;

// Alias, um Namenskonflikte mit SearchApi.Elastic zu vermeiden
using Ecs = global::Elastic.Clients.Elasticsearch;

using SearchApi.Elastic;   // für ElasticsearchService (dein eigener Namespace)
using SearchApi.Models;    // für das Request/Document-Model
using SearchApi.Parsing;   // für BoolParser + EsQueryBuilder

namespace SearchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SearchController : ControllerBase
{
    private readonly ElasticsearchService _es;

    public SearchController(ElasticsearchService es) => _es = es;

    [HttpPost]
    public async Task<IActionResult> Search([FromBody] SearchApi.Models.SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query must not be empty");

        // 1) ANTLR parse → AST
        var ast = BoolParser.Parse(request.Query);

        // 2) AST → Elasticsearch Query (QueryDsl.Query)
        var esQuery = EsQueryBuilder.ToEsQuery(ast);

        var size = request.Size <= 0 ? 10 : request.Size;

        // 3) Nicht-generischen Request verwenden (vermeidet Typargument-/Lambda-Probleme)
        var req = new Ecs.SearchRequest
        {
            Size  = size,
            Query = esQuery
        };

        // Optional: Index pro Anfrage setzen (sonst DefaultIndex aus der Config)
        if (!string.IsNullOrWhiteSpace(request.Index))
            req.Indices = request.Index;

        // 4) Suche ausführen – Typargument explizit angeben
        var response = await _es.Client.SearchAsync<SearchApi.Models.Document>(req);

        if (!response.IsSuccess())
            return StatusCode(500, response.ElasticsearchServerError?.ToString() ?? "Search failed");

        return Ok(new
        {
            took = response.Took,
            hits = response.Hits.Select(h => new
            {
                id = h.Id,
                score = h.Score,
                source = h.Source
            })
        });
    }
}
