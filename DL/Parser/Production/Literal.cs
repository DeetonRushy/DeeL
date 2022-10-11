using DL.Lexer;

namespace DL.Parser.Production;

public record Literal(DToken Sentiment) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitLiteral(this);
    }
}