using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins;

internal class EnvAddFunction : ICallable
{
    public int Arity => 2;

    public string Identifier => "envset";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        if (interpreter.VisitLiteral(args[0]) is not string name)
        {
            interpreter.DisplayErr($"environment variable names should be a string.");
            return Literal.False;
        }

        var value = interpreter.VisitLiteral(args[1]);
        Environment.SetEnvironmentVariable(name, value?.ToString());

        return Literal.True;
    }
}
