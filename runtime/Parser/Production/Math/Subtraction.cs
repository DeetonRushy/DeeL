
namespace Runtime.Parser.Production.Math;

public record Subtraction : MathStatement
{
    public Subtraction(Statement Left, Statement Right, int Line) : base(Left, Right, Line)
    {
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitSubtraction(this);
    }
}
