using DL.Lexer;

namespace DL.Parser.Production;

public record DictAssignment(Literal Key, DToken Colon, DNode Value) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitDictAssignment(this);
    }

    public override string Debug()
    {
        return $"DictElement(Key: {Key.Object})";
    }
}