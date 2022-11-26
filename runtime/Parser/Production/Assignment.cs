namespace Runtime.Parser.Production;

public record Assignment(Declaration Decl, Statement Statement) : Statement(Decl.Line)
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitAssignment(this);
    }

    public override string Debug()
    {
        return $"Variable(Name: '{Decl.Name}')";
    }
}