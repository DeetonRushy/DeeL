using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Parser.Production.Math;

public record Addition : MathStatement
{
    public Addition(Statement Left, Statement Right, int Line) : base(Left, Right, Line)
    {
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitAddition(this);
    }
}
