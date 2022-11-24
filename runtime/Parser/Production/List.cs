using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record List(DToken OpenBracket, Statement[] Literals, DToken CloseBracket, int Line) : Statement(Line)
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