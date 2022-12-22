namespace Runtime.Parser.Production;

public record FunctionCall(string Identifier, Statement[] Arguments, int Line) : Statement(true, Line)
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