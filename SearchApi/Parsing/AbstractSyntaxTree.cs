namespace SearchApi.Parsing;

using SearchApi.Parsing.Interfaces;

public sealed record WordNode(String Text) : INode;
public sealed record NotNode(INode Inner) : INode;
public sealed record AndNode(IReadOnlyList<INode> Items) : INode;
public sealed record OrNode(IReadOnlyList<INode> Items) : INode;
public sealed record PhraseNode(String Text) : INode;
public sealed record NearNode(INode Left, INode Right, Int32 Slop) :INode;