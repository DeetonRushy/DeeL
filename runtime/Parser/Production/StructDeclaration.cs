
namespace Runtime.Parser.Production;

/*
 a struct that can have member functions. No inheritance.
 */

public record StructDeclaration(string Identifier, List<Declaration> Declarations, int Line) : Declaration(Identifier, new TypeHint(Identifier), Line)
{
    public override string Debug()
    {
        return $"Struct({Identifier}, {Declarations.Count} members)";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitStructDeclaration(this);
    }
}
