using DL.Interpreting.Api;
using DL.Parser;
using DL.Parser.Production;

namespace DL.Interpreting.Calls;

public interface ICallable
{
    public string Identifier { get; }

    Literal Execute(ISyntaxTreeVisitor<DValue> interpreter, params Literal[] args);
}