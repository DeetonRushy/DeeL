using Runtime.Interpreting.Api;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls;

public interface ICallable
{
    public string Identifier { get; }

    Literal Execute(ISyntaxTreeVisitor<DValue> interpreter, params Literal[] args);
}