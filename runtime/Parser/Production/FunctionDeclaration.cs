
namespace Runtime.Parser.Production;

public record FunctionDeclaration(string Identifier, List<Variable> Arguments, Block Body): Statement
{
    public override string Debug()
    {
        return "FunctionDecl";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitFunctionDeclaration(this);
    }
}
