using DL.Lexer;

namespace DL.Parser.Production;

public record Literal(DToken Sentiment, object Object) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitLiteral(this);
    }

    public override void Debug()
    {
        Console.WriteLine($"Literal({Sentiment.Type}): {Object}");
    }
}