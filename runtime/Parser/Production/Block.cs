using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Parser.Production;

// a block is not a statement, this wording is fucked
public record Block(List<Statement> Statements) : Statement
{
    public override string Debug()
    {
        throw new NotImplementedException();
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitBlock(this);
    }
}
