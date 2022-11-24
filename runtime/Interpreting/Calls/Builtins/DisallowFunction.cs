using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Interpreting.Calls.Builtins;

internal class DisallowFunction : ICallable
{
    public int Arity => -1;

    public string Identifier => "disallow";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        if (interpreter is not Interpreter unit)
        {
            throw new InterpreterException($"cannot disallow from interpreter that does not support flags.");
        }

        foreach (var arg in args.Select(n => interpreter.VisitLiteral(n)))
        {
            unit.ModuleFlags[arg.ToString()!] = false;
        }

        return new Literal(DToken.MakeVar(TokenType.Number), TypeHint.Integer, args.Length);
    }
}
