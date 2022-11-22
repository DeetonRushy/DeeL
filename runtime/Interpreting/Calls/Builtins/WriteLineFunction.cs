using Newtonsoft.Json;
using Runtime.Parser.Production;
using System.Text;

namespace Runtime.Interpreting.Calls.Builtins;

internal class WriteLineFunction : ICallable
{
    public int Arity => -1;

    public string Identifier => "writeln";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        var sb = new StringBuilder();
        foreach (var literal in args)
        {
            if (literal.Object is Dictionary<object, object> or List<object>)
            {
                var json = JsonConvert.SerializeObject(literal.Object);
                sb.Append(json);
                continue;
            }
            sb.Append($"{literal.Object} ");
        }
        if (interpreter.AllowsStdout)
            Console.WriteLine(sb.ToString());
        return Literal.Undefined;
    }
}
