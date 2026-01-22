using Elastic.Clients.Elasticsearch.QueryDsl;
using Antlr4.Runtime;

namespace SearchApi.Parsing;

using global::Elastic.Clients.Elasticsearch;
using SearchApi.Parsing.Interfaces;

public static class BoolParser
{
    public static INode Parse(String input)
    {
        AntlrInputStream inputStream = new(input);
        BoolQueryLexer lexer = new(inputStream);
        CommonTokenStream tokens = new(lexer);
        BoolQueryParser parser = new(tokens);

        parser.RemoveErrorListeners();
        parser.ErrorHandler = new BailErrorStrategy();

        BoolQueryParser.QueryContext? tree = parser.query();
        return new AstBuilderVisitor().Visit(tree.orExpr());
    }
}

internal class AstBuilderVisitor : BoolQueryBaseVisitor<INode>
{
    public override INode VisitOrExpr(BoolQueryParser.OrExprContext context)
    {
        List<INode> items =
        [
            Visit(context.andExpr(0)),
        ];
        for (Int32 i = 1; i < context.andExpr().Length; i++)
            items.Add(Visit(context.andExpr(i)));
        return items.Count == 1 ? items[0] : new OrNode(items);
    }

    public override INode VisitAndExpr(BoolQueryParser.AndExprContext context)
    {
        List<INode> items = [Visit(context.term(0))];
        for (Int32 i = 1; i < context.term().Length; i++)
            items.Add(Visit(context.term(i)));
        return items.Count == 1 ? items[0] : new AndNode(items);
    }

    public override INode VisitTerm(BoolQueryParser.TermContext context)
    {
        INode? node = Visit(context.factor());
        return context.NOT() != null ? new NotNode(node) : node;
    }

    public override INode VisitFactor(BoolQueryParser.FactorContext context)
    {
        if (context.PHRASE() != null)
        {
            String? raw = context.PHRASE().GetText();
            String text = raw.Substring(1, raw.Length - 2);
            return new PhraseNode(text);
        }

        if (context.WORD() != null)
        {
            return new WordNode(context.WORD().GetText());
        }

        if (context.nearCall() != null)
        {
            return Visit(context.nearCall());
        }

        return Visit(context.orExpr());
    }

    public override INode VisitNearCall(BoolQueryParser.NearCallContext context)
    {
        INode? left = Visit(context.factor(0));
        INode? right = Visit(context.factor(1));

        Int32 slop = 5;
        if (context.NUMBER() != null)
        {
            slop = Int32.Parse(context.NUMBER().GetText());
        }

        return new NearNode(left, right, slop);
    }
}

public static class EsQueryBuilder
{
    private static readonly Field[] _fields =
    [
        new("title"),
        new("description"),
        new("text"),
    ];

    public static Query ToEsQuery(INode node) =>
        node switch
        {
            PhraseNode p => new MatchPhraseQuery()
            {
                Field = "text",
                Query = p.Text,
            },

            WordNode w => new MultiMatchQuery
            {
                Query = w.Text,
                Fields = _fields,
                Type = TextQueryType.BestFields,
            },

            OrNode o => new BoolQuery()
            {
                Should = o.Items.Select(ToEsQuery).ToList(),
                MinimumShouldMatch = 1,
            },

            AndNode a => new BoolQuery()
            {
                Must = a.Items.Select(ToEsQuery).ToList(),
            },

            NotNode n => new BoolQuery
            {
                MustNot = new List<Query> {ToEsQuery(n.Inner)},
            },

            NearNode n => new MatchPhraseQuery
            {
                Field = "text",
                Query = $"{ExtractText(n.Left)}, {ExtractText(n.Right)}",
                Slop = n.Slop,
            },

            var _ => new MatchAllQuery(),
        };

    private static String ExtractText(INode node) => node switch
    {
        WordNode w => w.Text,
        PhraseNode p => p.Text,
        var _ => throw new InvalidOperationException("Near() erlaubt aktuell nur WORD oder PHRASE als Argument."),
    };
}