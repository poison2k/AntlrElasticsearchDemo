using System.Net;
using System.Net.Http.Json; 
using FluentAssertions; 
using Reqnroll; 
using SearchApi.Models; 
using System.Text.Json;
using SearchApi.IntegrationTests.Models;

namespace SearchApi.IntegrationTests.Steps;

[Binding] 
public sealed class SearchSteps(ScenarioContext ctx)
{
    private HttpClient Http => ctx.Get<HttpClient>(); 
    private HttpResponseMessage? _response;
    private String _responseBody = String.Empty;
    
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Given("""the index "(.*)" exists""")] 
    public async Task GivenTheIndexExists(string index) 
    { 
        HttpResponseMessage res = await Http.PostAsJsonAsync("/api/admin/index", new { name = index }); 
        res.StatusCode.Should().Be(HttpStatusCode.OK); 
    } 
    
    [Given(@"the following documents are indexed:")] 
    public async Task GivenTheFollowingDocumentsAreIndexed(Table table) 
    { 
        Document[] docs = table.Rows.Select(r => new Document 
        { 
            Id = r["id"], 
            Title = r["title"], 
            Description = r["description"], 
            Text = r["text"], 
        }).ToArray(); 
        
        HttpResponseMessage res = await Http.PostAsJsonAsync("/api/admin/seed", new { index = "documents",  docs }); 
        res.StatusCode.Should().Be(HttpStatusCode.OK); 
    } 
    

    [When("""
          I search with query "(.*)"
          """)] 
    public async Task WhenISearchWithQuery(String q) 
    { 
        _response = await Http.PostAsJsonAsync("/api/search", new { index = "documents", query = q, size = 50 });
        _responseBody = await _response.Content.ReadAsStringAsync();
    } 

    [Then(@"the response status should be (.*)")] 
    public void ThenTheResponseStatusShouldBe(Int32 code) 
        => ((Int32)_response!.StatusCode).Should().Be(code); 
    
    [Then(@"the result ids should contain:")] 
    public async Task ThenTheResultIdsShouldContain(Table table)
    {
        HashSet<String> expected = table.Rows.Select(r => r["id"]).ToHashSet();

        SearchResponseDto? dto = JsonSerializer.Deserialize<SearchResponseDto>(_responseBody, _options);
        dto.Should().NotBeNull();

        HashSet<String> actual = dto!.Hits.Select(h => h.Id).ToHashSet();
        actual.Should().Contain(expected);
    } 

} 