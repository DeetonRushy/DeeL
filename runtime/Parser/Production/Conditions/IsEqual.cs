
namespace Runtime.Parser.Production.Conditions;

public record IsEqual : Condition
{
    public IsEqual(Statement Left, Statement Right) : base(Left, Right)
    {
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitIsEqualsComparison(this);
    }
}
