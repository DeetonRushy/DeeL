using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record DictAssignment(Literal Key, DToken Colon, Statement Value, int Line) : Statement(Line)
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