namespace Runtime.Parser.Production;

public record ModuleImport(string FileName, string[] Members, int Line): Statement(Line)
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