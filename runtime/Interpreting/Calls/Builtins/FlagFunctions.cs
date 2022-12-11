using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins;

internal class DisableFunction : ICallable
{
    public int Arity => -1;

    public string Identifier => "disable";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        foreach (var arg in args.Select(n => interpreter.VisitLiteral(n)))
        {
            interpreter.ModuleFlags[arg.ToString()!] = false;
        }

        return new Literal(DToken.MakeVar(TokenType.Number), TypeHint.Integer, args.Length);
    }
}

internal class EnableFunction : ICallable
{
    public int Arity => 1;
    public string Identifier => "enable";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        foreach (var arg in args.Select(n => interpreter.VisitLiteral(n)))
        {
            interpreter.ModuleFlags[arg.ToString()!] = true;
        }

        return new Literal(DToken.MakeVar(TokenType.Number), TypeHint.Integer, args.Length);
    }
}
