using System.Text;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Structs.Builtin;

public class StringBuiltin : BaseBuiltinStructDefinition
{
    public override string Name => "String";

    public StringBuiltin()
        : base("StringHelper")
    {
        DefineBuiltinFunction("format", true, ExecuteFormat);
    }

    public ReturnValue ExecuteFormat(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        if (args.Count == 0)
        {
            interpreter.Panic("expected at least one argument");
        }

        if (args.First().Take(interpreter) is not string format)
        {
            interpreter.Panic("String::format(): expect arg0 to be the format string.");
            return null!;
        }

        var vaArgs = args.Skip(1).ToList();
        var cursor = 0;
        bool wasLastFormatDelim = false;
        
        var sb = new StringBuilder();

        for (var i = 0; i < format.Length; ++i)
        {
            if (wasLastFormatDelim)
            {
                if (format[i] != '}')
                    interpreter.Panic("invalid format string. [expected '}']");
                wasLastFormatDelim = false;
                continue;
            }
            
            if (format[i] == '{')
            {
                if ((cursor + 1) > vaArgs.Count)
                    interpreter.Panic($"format delimiter #{cursor + 1} does not have a matching argument");
                sb.Append(vaArgs[cursor++].Take(interpreter));
                wasLastFormatDelim = true;
                continue;
            }

            sb.Append(format[i]);
        }

        return new ReturnValue(sb.ToString(), 0);
    }
}