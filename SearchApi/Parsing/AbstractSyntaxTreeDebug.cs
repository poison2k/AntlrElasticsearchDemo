namespace SearchApi.Parsing;

public static class AbstractSyntaxTreeDebug
{
    public static String Dump(INode n) => n
        switch
        {
            WordNode w => w.Text,
            PhraseNode p => $"\"{p.Text}\"",
            NotNode x => $"NOT({Dump(x.Inner)})",
            AndNode a => $"AND[{String.Join(", ", a.Items.Select(Dump))}]",
            OrNode o => $"OR[{String.Join(", ", o.Items.Select(Dump))}]",
            NearNode near => $"NEAR({Dump(near.Left)}, {Dump(near.Right)}: {near.Slop})",
            var _ => "<?>",
        };
}