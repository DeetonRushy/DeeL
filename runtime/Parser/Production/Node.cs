namespace Runtime.Parser.Production;

public abstract record Statement
{
    public abstract T Take<T>(ISyntaxTreeVisitor<T> visitor);
    public abstract string Debug();
}