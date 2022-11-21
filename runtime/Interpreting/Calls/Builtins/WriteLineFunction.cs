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
    public int Arity => -1;

    public string Identifier => "writeln";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        var sb = new StringBuilder();
        foreach (var literal in args)
            sb.Append($"{literal} ");
        Console.WriteLine(sb.ToString());
        return Literal.Undefined;
    }
}
