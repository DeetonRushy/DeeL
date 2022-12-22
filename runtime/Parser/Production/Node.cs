namespace Runtime.Parser.Production;

public abstract record Statement(bool IsConst, int Line)
{
    public abstract T Take<T>(ISyntaxTreeVisitor<T> visitor);
    public abstract string Debug();
}