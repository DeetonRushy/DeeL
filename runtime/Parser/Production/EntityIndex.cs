
namespace Runtime.Parser.Production;

public record EntityIndex(Statement Entity, List<Statement> Indices, int Line) : Statement(true, Line)
{
    public override string Debug()
    {
        throw new NotImplementedException();
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitEntityIndex(this);
    }
}
