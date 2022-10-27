namespace Runtime.Parser.Production;

public abstract record DNode
{
    public abstract T Take<T>(ISyntaxTreeVisitor<T> visitor);
    public abstract string Debug();
}