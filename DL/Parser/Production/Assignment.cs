using DL.Lexer;

namespace DL.Parser.Production;

public record Assignment(DNode Key, DNode Value) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitAssignment(this);
    }

    public override string Debug()
    {
        // value could be anything, so attempt to
        // display the key. (key can only be str, num, dec)

        var identifier = Key as Literal;

        if (identifier is null)
        {
            throw new 
                NotImplementedException("somehow, some way, an assignments key is null...");
        }
        
        if (Value is Literal value)
        {
            return $"Assignment(Id: {identifier.Object}, Value: {value.Object})";
        }

        if (Value is List list)
        {
            return $"Assignment(Id: {identifier.Object}, Value: {list.Debug()})";
        }

        if (Value is Dict dict)
        {
            return $"Assignment(Id: {identifier.Object}, Value: {dict.Debug()}";
        }

        return $"Assignment(Id: {identifier.Object}, Value: <unknown>";
    }
}