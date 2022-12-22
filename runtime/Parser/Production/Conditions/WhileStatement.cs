
namespace Runtime.Parser.Production.Conditions;

public record WhileStatement(Condition Condition, Block Body, int Line) : Statement(true, Line)
{
    public override string Debug()
    {
        return "While";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitWhileLoop(this);
    }
}
