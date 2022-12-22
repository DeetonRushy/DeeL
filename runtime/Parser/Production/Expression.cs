namespace Runtime.Parser.Production;

// This inherits from Statement, which looks weird. But statement is basically just `Node`.

public record Expression(int Line): Statement(false, Line)
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    public override string Debug()
    {
        throw new NotImplementedException();
    }
}