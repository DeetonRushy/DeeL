
namespace Runtime.Parser.Production.Conditions;

public record Condition(Statement Left, Statement Right) : Statement
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
