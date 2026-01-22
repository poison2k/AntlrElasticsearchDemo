namespace SearchApi.UnitTests.Elastic;

using System.Reflection.Metadata;
using FluentAssertions;
using global::Elastic.Clients.Elasticsearch.QueryDsl;
using SearchApi.Parsing;
using SearchApi.Parsing.Interfaces;
using SearchApi.UnitTests.TestUtils;

public class EsQueryBuilderTests
{
   [Test]
   public async Task Or_should_create_should_clause()
    {
        INode ast = BoolParser.Parse("one OR two");
        Query esQuery = EsQueryBuilder.ToEsQuery(ast);
        String json = await EsSerialize.CaptureSearchRequestBodyAsync<Document>(client => 
            client.SearchAsync<Document>(s => 
                s.Indices("documents")
                    .Size(10)
                    .Query(esQuery)
                )
            );
        json.Should().Contain("\"should\"");
        json.Should().NotContain("\"must\"");
    }
}