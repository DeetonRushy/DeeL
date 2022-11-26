using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls;

public interface ICallable
{
    public int Arity { get; }

    public string Identifier { get; }

    Literal Execute(Interpreter interpreter, params Literal[] args);
}