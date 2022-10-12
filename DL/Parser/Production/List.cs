using DL.Lexer;

namespace DL.Parser.Production;

public record List(DToken OpenBracket, Literal[] Literals, DToken CloseBracket) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitList(this);
    }

    public override void Debug()
    {
        Console.WriteLine($"List({Literals.Length} items)");
    }
}