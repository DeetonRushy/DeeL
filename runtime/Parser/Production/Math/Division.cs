

namespace Runtime.Parser.Production.Math;

public record Division : MathStatement
{
    public Division(Statement Left, Statement Right) : base(Left, Right)
    {
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitDivision(this);
    }
}
