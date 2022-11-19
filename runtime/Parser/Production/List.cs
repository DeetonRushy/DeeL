using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record List(DToken OpenBracket, Literal[] Literals, DToken CloseBracket) : Statement
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitList(this);
    }

    public override string Debug()
    {
        return $"List({Literals.Length} items)";
    }
}