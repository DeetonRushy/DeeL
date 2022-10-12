using DL.Lexer;

namespace DL.Parser.Production;

public record DictAssignment(Literal Key, DToken Colon, Literal Value) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitDictAssignment(this);
    }

    public override void Debug()
    {
        Console.WriteLine($"DictElement(Key: {Key.Object}, Value: {Value.Object})");
    }
}