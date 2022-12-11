using Runtime.Parser.Production;
using Runtime.Interpreting.Structs.Builtin;
using Runtime.Lexer;
using Runtime.Parser;

namespace Runtime.Interpreting.Calls.Builtins;

internal class WriteLineFunction : ICallable
{
    public int Arity => -1;

    public string Identifier => "print";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        var formatted = StringBuiltin.ExecuteFormat(
            interpreter, 
            null!, 
            args.Select(x => x as Statement).ToList());

        if (interpreter.AllowsStdout)
        {
            Console.WriteLine(formatted);
        }

        return new Literal(DToken.MakeVar(TokenType.Number), TypeHint.Integer, 0);
    }
}
