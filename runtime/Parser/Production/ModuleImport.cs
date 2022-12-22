namespace Runtime.Parser.Production;

public record ModuleImport(string FileName, string[] Members, int Line): Statement(true, Line)
{
    public override string Debug()
    {
        return $"Import({FileName})";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitModuleImport(this);
    }
}

public record AssignedModuleImport(Variable Assignee, string Name, int Line) : Statement(true, Line)
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitModuleAssignment(this);
    }

    public override string Debug()
    {
        return $"Namespace(Name: {Assignee.Name}, Contents: <from '{Name}'>)";
    }
}