namespace Runtime.Parser.Production;

public record VariableAccessAssignment(VariableAccess Access, TypeHint Hint, Statement Operand, int Line) : Statement(Line)
{
    public override string Debug()
    {
        throw new NotImplementedException();
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitVariableAccessAssignment(this);
    }
}
