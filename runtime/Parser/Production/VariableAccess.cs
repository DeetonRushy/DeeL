
namespace Runtime.Parser.Production;

/*
 a::b::c::d
 ^^^^^^^^^^
 Tree must be in order, start to finish.
 */

public record VariableAccess(List<Statement> Tree) : Statement
{

    public override string Debug()
    {
        return $"{string.Join("::", Tree.Select(x => x.Debug()))}";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitVariableAccess(this ,out _);
    }
}
