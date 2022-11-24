
namespace Runtime.Parser.Production.Math;

public record Multiplication : MathStatement
{
    public Multiplication(Statement Left, Statement Right, int Line) : base(Left, Right, Line)
    {
    }

    public override string Debug()
    {
        return $"Multiplication";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitMultiplication(this);
    }
}
