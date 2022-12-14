
namespace Runtime.Parser.Production;

public record ExplicitBreakpoint(int Line) : Statement(true, Line)
{
    public override string Debug()
    {
        return "BreakPoint";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitBreakPoint(this);
    }
}
