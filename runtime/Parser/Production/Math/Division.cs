

namespace Runtime.Parser.Production.Math;

public record Division : MathStatement
{
    public Division(Statement Left, Statement Right, int Line) : base(Left, Right, Line)
    {
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitDivision(this);
    }
}
