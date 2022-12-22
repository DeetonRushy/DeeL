namespace Runtime.Parser.Production;

public record VariableAccessAssignment(VariableAccess Access, TypeHint Hint, Statement Operand, bool IsConst, int Line) : Statement(IsConst, Line)
{
    public override string Debug()
    {
        return $"VariableAccessAssignment";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitVariableAccessAssignment(this);
    }
}
