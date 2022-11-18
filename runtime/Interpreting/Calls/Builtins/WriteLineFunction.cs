using Runtime.Lexer;
using Runtime.Parser.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Interpreting.Calls.Builtins;

internal class WriteLineFunction : ICallable
{
    public int Arity => 1;

    public string Identifier => "writeln";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        var message = interpreter.VisitLiteral(args[0]);
        if (message is not string msg)
        {
            interpreter.DisplayErr($"{Identifier}: writeln expects a string.");
            return new Literal(DToken.Bad, "");
        }
        interpreter.ModLog(msg);
        return new Literal(DToken.MakeVar(TokenType.Null), Interpreter.Undefined);
    }
}
