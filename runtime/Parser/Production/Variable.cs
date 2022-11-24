
using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record Variable(string Name, TypeHint Type, bool IsInitialization = true): Declaration(Name, Type)
{
    public override string Debug()
    {
        return $"Variable(Name: {Name})";
    }
                                          
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitVariable(this);
    }
}
