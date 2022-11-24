
namespace Runtime.Parser.Production.Conditions;

public record WhileStatement(Condition Condition, Block Body, int Line) : Statement(Line)
{
    public override string Debug()
    {
        throw new NotImplementedException();
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitWhileLoop(this);
    }
}
