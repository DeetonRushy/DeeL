
namespace Runtime.Parser.Production.Conditions;

public record Condition(Statement Left, Statement Right, int Line) : Statement(Line)
{
    public override string Debug()
    {
        throw new NotImplementedException();
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }
}
