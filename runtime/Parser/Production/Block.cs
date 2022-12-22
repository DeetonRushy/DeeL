namespace Runtime.Parser.Production;

// a block is not a statement, this wording is fucked
// line zero because each statement will be processed individually.
public record Block(List<Statement> Statements) : Statement(false, 0)
{
    public override string Debug()
    {
        return $"  ^^ (...{Statements.Count} lines of code)";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitBlock(this);
    }
}
