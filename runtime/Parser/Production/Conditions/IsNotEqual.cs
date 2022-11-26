namespace Runtime.Parser.Production.Conditions;

public record IsNotEqual : Condition
{
    public IsNotEqual(Statement Left, Statement Right, int Line) : base(Left, Right, Line)
    {
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitIsNotEquals(this);
    }
}
