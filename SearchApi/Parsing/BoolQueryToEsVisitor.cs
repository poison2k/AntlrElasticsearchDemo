using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SearchApi.Parsing;

public static class BoolParser
{
    public static Node Parse(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new BoolQueryLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new BoolQueryParser(tokens);

        parser.RemoveErrorListeners();
        parser.ErrorHandler = new BailErrorStrategy(); 

        var tree = parser.query();
        return new AstBuilderVisitor().Visit(tree);
    }
}


internal sealed class AstBuilderVisitor : BoolQueryBaseVisitor<Node>
{
    public override Node VisitWord(BoolQueryParser.WordContext context)
        => new WordNode(context.WORD().GetText());

    public override Node VisitNotTerm(BoolQueryParser.NotTermContext context)
    {
        var factor = context.factor();
        var node = Visit(factor);
        return context.NOT() != null ? new NotNode(node) : node;
    }

    public override Node VisitAndExpr(BoolQueryParser.AndExprContext context)
        => new AndNode(Visit(context.expr()), Visit(context.term()));

    public override Node VisitOrExpr(BoolQueryParser.OrExprContext context)
        => new OrNode(Visit(context.expr()), Visit(context.term()));

    public override Node VisitToTerm(BoolQueryParser.ToTermContext context)
        => Visit(context.term());

    public override Node VisitParen(BoolQueryParser.ParenContext context)
        => Visit(context.expr());
}

public static class EsQueryBuilder
{
    private const string DefaultField = "content";

    public static Query ToEsQuery(Node node) =>
        node switch
        {
            WordNode w => new MatchQuery
            {
                Field = DefaultField,   // required!
                Query = w.Text
            },

            NotNode n => new BoolQuery
            {
                MustNot = new List<Query> { ToEsQuery(n.Inner) }
            },

            AndNode a => MergeBool(a.Left, a.Right, and: true),
            OrNode  o => MergeBool(o.Left, o.Right, and: false),

            _ => new MatchAllQuery()
        };

    private static Query MergeBool(Node left, Node right, bool and)
    {
        var l = ToEsQuery(left);
        var r = ToEsQuery(right);

        if (and)
            return new BoolQuery { Must = new List<Query> { l, r } };

        return new BoolQuery
        {
            Should = new List<Query> { l, r },
            MinimumShouldMatch = 1
        };
    }
}