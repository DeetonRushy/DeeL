using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins;

public class PanicFunction : ICallable
{
    public int Arity => 1;
    public string Identifier => "panic";
    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        if (args.First().Take(interpreter) is not string s)
        {
            interpreter.Panic("invalid call to `panic`, this function expects a string argument.");
            return null!;
        }
        
        interpreter.Panic(s);
        return null!;
    }
}