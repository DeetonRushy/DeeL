using DL.Lexer;

namespace DL.Parser.Production;

public record FunctionCall(string Identifier, Literal[] Arguments) : DNode
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