using DL.Lexer;

namespace DL.Parser.Production;

public record Assignment(DNode Key, DNode Value) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitAssignment(this);
    }

    public override void Debug()
    {
        // value could be anything, so attempt to
        // display the key. (key can only be str, num, dec)

        if (Key is Literal literal)
        {
            Console.WriteLine($"Assignment(Id: {literal.Object})");
        }
    }
}