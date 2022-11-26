namespace Runtime.Parser.Production;

public record Variable(string Name, TypeHint Type, int Line, bool IsInitialization = true) : Declaration(Name, Type, Line)
{
    public override string Debug()
    {
        return $"Variable(Name: {Name})";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitVariable(this);
    }

    public bool IsAssignableTo(Variable other)
        => other.Type == Type;
}
