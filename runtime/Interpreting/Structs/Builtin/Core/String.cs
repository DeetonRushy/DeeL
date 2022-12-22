using System.Text;
using Newtonsoft.Json;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Structs.Builtin;

public class StringBuiltin : BaseBuiltinStructDefinition
{
    public override string Name => "String";

    public StringBuiltin()
        : base("StringHelper")
    {
        DefineBuiltinFunction("format", true, ExecuteFormat);
        DefineBuiltinFunction("make", true, ExecuteFrom);
    }

    public static ReturnValue ExecuteFrom(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        if (args.Count != 1)
        {
            interpreter.Panic("expected one argument");
        }

        var statement = args[0].Take(interpreter);

        if (Configuration.GetFlag("auto-serialize-all") ||
            statement is List<object> or Dictionary<object, object>)
        {
            return new ReturnValue(JsonConvert.SerializeObject(statement), 0);
        }

        return new ReturnValue(statement.ToString(), 0);
    }

    public static ReturnValue ExecuteFormat(Interpreter interpreter, IStruct self, List<Statement> args)
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
                var instance = vaArgs[cursor++].Take(interpreter);
                
                if (instance is List<object> or IDictionary<object, object> 
                    || Configuration.GetFlag("auto-serialize-all"))
                {
                    var serialized = JsonConvert.SerializeObject(instance);
                    sb.Append(serialized);
                }
                else if (instance is IStruct @struct)
                {
                    if (@struct.GetValue("to_string") is IStructFunction fn)
                    {
                        sb.Append(fn.Execute(interpreter, @struct, Array.Empty<Statement>().ToList()).Value);
                    }
                    else
                    {
                        sb.Append(@struct);
                    }
                }
                else
                {
                    sb.Append(instance);
                }
                wasLastFormatDelim = true;
                continue;
            }

            sb.Append(format[i]);
        }

        return new ReturnValue(sb.ToString(), 0);
    }
}