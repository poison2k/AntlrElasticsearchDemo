namespace SearchApi.Parsing;
public abstract record Node;

public sealed record WordNode(string Text) : Node;
public sealed record NotNode(Node Inner) : Node;
public sealed record AndNode(Node Left, Node Right) : Node;
public sealed record OrNode(Node Left, Node Right) : Node;