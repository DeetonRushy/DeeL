using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Parser.Production;

// a block is not a statement, this wording is fucked
// line zero because each statement will be processed individually.
public record Block(List<Statement> Statements) : Statement(0)
{
    public override string Debug()
    {
        return $"  ^^ (...{Statements.Count} lines of code)";
    }

    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitBlock(this);
    }
}
