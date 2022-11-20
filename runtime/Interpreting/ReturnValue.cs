
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting;

public record ReturnValue(object? Value) : Statement
{
    public override string Debug()
    {
        return $"ReturnStatement(Value: '{Value}')";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitReturnStatement(this);
    }
}
