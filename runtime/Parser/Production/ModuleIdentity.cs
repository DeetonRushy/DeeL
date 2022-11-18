
namespace Runtime.Parser.Production;

public record ModuleIdentity(Literal ModuleName) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitModuleIdentity(this);
    }

    public override string Debug()
    {
        return $"ModuleIdentity(Name: '{ModuleName.Object}')";
    }
}
