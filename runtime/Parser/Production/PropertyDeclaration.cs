namespace Runtime.Parser.Production;

public record PropertyDeclaration(string Name,
    bool IsStatic,
    TypeHint Type,
    Expression? Initializer,
    int Line
): Declaration(Name, Type, Line)
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitPropertyDeclaration(this);
    }

    public override string Debug()
    {
        return "Property";
    }
}