
namespace Runtime.Parser.Production;

/*
 a::b::c::d
 ^^^^^^^^^^
 Tree must be in order, start to finish.
 */

public record VariableAccess(List<Variable> Tree) : Statement
{

    public override string Debug()
    {
        throw new NotImplementedException();
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitVariableAccess(this ,out _);
    }
}
