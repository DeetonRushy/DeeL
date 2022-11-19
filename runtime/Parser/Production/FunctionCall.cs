using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record FunctionCall(string Identifier, DNode[] Arguments) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitFunctionCall(this);
    }

    public override string Debug()
    {
        return $"Call({Identifier}, {Arguments.Length} args)";
    }
}