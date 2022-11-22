
namespace Runtime.Parser.Production;

public record Conditional(Statement Condition, Block SuccessBlock, Block FallbackBlock) : Statement
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
