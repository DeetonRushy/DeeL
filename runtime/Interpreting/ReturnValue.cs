
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting;

public record ReturnValue(object? Value, int Line) : Statement(Line)
{
    public override string Debug()
    {
        return $"ReturnStatement(Value: '{Value}')";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitReturnStatement(this);
    }

    public override string ToString()
    {
        return Value?.ToString() ?? "null";
    }

    public static ReturnValue Bad => new(0, 0);
}
