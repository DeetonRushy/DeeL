
namespace Runtime.Parser.Production;

public record FunctionDeclaration(string Identifier, List<Variable> Arguments, Block Body, TypeHint TypeHint): Declaration
{
    public override string Debug()
    {
        return $"FunctionDecl({Identifier}, {Arguments.Count} args)\n{Body.Debug()}";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitFunctionDeclaration(this);
    }
}
