
namespace Runtime.Parser.Production;

public record Grouping(List<Statement> Statements, int Line) : Expression(Line)
{
    public override string Debug()
    {
        throw new NotImplementedException();
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitGrouping(this);
    }
}
