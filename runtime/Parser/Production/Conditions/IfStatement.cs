namespace Runtime.Parser.Production.Conditions;

public record IfStatement(Statement Condition, Block SuccessBlock, Block? FallbackBlock, bool IsConst, int Line) : Statement(IsConst, Line)
{
    public override string Debug()
    {
        return $"IfStatement";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitIfStatement(this);
    }
}
