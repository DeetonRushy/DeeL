
namespace Runtime.Parser.Production.Conditions;

public record IsEqual : Condition
{
    public IsEqual(Statement Left, Statement Right, int Line) : base(Left, Right, Line)
    {
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitIsEqualsComparison(this);
    }
}
