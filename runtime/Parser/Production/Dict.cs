using DL.Lexer;

namespace DL.Parser.Production;

public record Dict(DToken OpenBrace, DictAssignment[] Members, DToken CloseBrace) : DNode
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