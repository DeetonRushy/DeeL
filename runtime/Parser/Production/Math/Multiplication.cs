
namespace Runtime.Parser.Production.Math;

public record Multiplication : MathStatement
{
    public Multiplication(Statement Left, Statement Right) : base(Left, Right)
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
