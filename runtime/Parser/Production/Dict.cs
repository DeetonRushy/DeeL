using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record Dict(DToken OpenBrace, DictAssignment[] Members, DToken CloseBrace, int Line) : Statement(Line)
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitDict(this);
    }

    public override string Debug()
    {
        return $"Dict({Members.Length} items)";
    }
}