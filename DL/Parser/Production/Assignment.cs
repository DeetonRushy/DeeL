using DL.Lexer;

namespace DL.Parser.Production;

public record Assignment(DToken Key, DToken Value) : DNode
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitAssignment(this);
    }
}