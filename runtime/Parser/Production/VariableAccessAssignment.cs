
using System.Linq.Expressions;

namespace Runtime.Parser.Production;

public record VariableAccessAssignment(VariableAccess Access, Statement Operand) : Statement
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
