namespace SearchApi.UnitTests.Parsing;

using FluentAssertions;
using SearchApi.Parsing;
using SearchApi.Parsing.Interfaces;

public class NearPhraseTests
{
    [Test]
    public void Parse_phrase_should_work()
    {
        // act 
        INode ast = BoolParser.Parse("\"quick brown fox\"");

        // assert
        AbstractSyntaxTreeDebug.Dump(ast).Should().Be("\"quick brown fox\"");
    }
    
    [Test]
    public void Near_default_slop_is_5()
    {
        // act 
        INode ast = BoolParser.Parse("Near(apple, banana)");

        // assert
        AbstractSyntaxTreeDebug.Dump(ast).Should().Contain("5");
    }
    
    [Test]
    public void Near_custom_slop()
    {
        // act 
        INode ast = BoolParser.Parse("Near(apple, banana; 12)");

        // assert
        AbstractSyntaxTreeDebug.Dump(ast).Should().Contain("12");
    }
}