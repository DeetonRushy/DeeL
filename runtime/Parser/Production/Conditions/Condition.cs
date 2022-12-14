
namespace Runtime.Parser.Production.Conditions;

public record Condition(Statement Left, Statement Right, int Line) : Statement(false, Line)
{
    public override string Debug()
    {
        return $"Conditional(Left: {Left.Debug()}, Right: {Right.Debug()})";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        throw new InvalidOperationException("You must override Condition.Take");
    }
}
