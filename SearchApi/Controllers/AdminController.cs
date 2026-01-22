using Microsoft.AspNetCore.Mvc;
using SearchApi.Elastic;
using SearchApi.Models;

namespace SearchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AdminController(ElasticsearchService es) : ControllerBase
{
    [HttpPost("index")]
    public async Task<IActionResult> CreateIndex([FromBody] CreateIndexRequest request)
    {
        Boolean ok = await es.EnsureIndexAsync(request.Name);
        return ok ? Ok(new { created = true, index = request.Name }) : StatusCode(500, "Index creation failed");
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed([FromBody] SeedBody body)
    {
        if (body.Docs.Length == 0)
            return BadRequest("No documents provided");


        await es.IndexManyAsync(body.Index ?? "documents", body.Docs);
        return Ok(new { indexed = body.Docs.Length });
    }
    
   
}