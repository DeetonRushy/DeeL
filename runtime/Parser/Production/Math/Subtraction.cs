
namespace Runtime.Parser.Production.Math;

public record Subtraction : MathStatement
{
    public Subtraction(Statement Left, Statement Right) : base(Left, Right)
    {
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitSubtraction(this);
    }
}
