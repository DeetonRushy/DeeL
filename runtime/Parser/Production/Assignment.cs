using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record Assignment(Declaration Declaration, Statement Statement) : Statement
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitAssignment(this);
    }

    public override string Debug()
    {
        return $"Variable(Name: '{Declaration.Name}')";
    }
}