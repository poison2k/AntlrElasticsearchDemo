namespace SearchApi.UnitTests.Parsing;

using FluentAssertions;
using SearchApi.Parsing;
using SearchApi.Parsing.Interfaces;

public class BoolParserTests
{
    [Test]
    public void Parse_or_chain_should_create_single_OrNode()
    {
        // act
        INode ast = BoolParser.Parse("one Or two Or three");

        // assert
        AbstractSyntaxTreeDebug.Dump(ast).Should().Be("OR[one, two, three]");
    }
    
    [Test]
    public void Parse_and_has_higher_precendence_then_or()
    {
        // act
        INode ast = BoolParser.Parse("one Or two And three");

        // assert
        AbstractSyntaxTreeDebug.Dump(ast).Should().Be("OR[one, AND[two, three]]");
    }
}