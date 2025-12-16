using Microsoft.AspNetCore.Mvc;
using SearchApi.Elastic;
using SearchApi.Models;

namespace SearchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AdminController : ControllerBase
{
    private readonly ElasticsearchService _es;
    
    public AdminController(ElasticsearchService es) => _es = es;
    
    [HttpPost("index")]
    public async Task<IActionResult> CreateIndex([FromBody] CreateIndexRequest request)
    {
        var ok = await _es.EnsureIndexAsync(request.Name);
        return ok ? Ok(new { created = true, index = request.Name }) : StatusCode(500, "Index creation failed");
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed([FromBody] SeedBody body)
    {
        if (body.Docs is null || body.Docs.Length == 0)
            return BadRequest("No documents provided");


        await _es.IndexManyAsync(body.Index ?? "documents", body.Docs);
        return Ok(new { indexed = body.Docs.Length });
    }
    
    public sealed class SeedBody
    {
        public string? Index { get; set; }
        public Document[] Docs { get; set; } = Array.Empty<Document>();
    }
}