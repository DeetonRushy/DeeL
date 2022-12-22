using System.Reflection;
using Runtime.Interpreting.Calls.Builtins;
using Runtime.Interpreting.Extensions;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Structs.Builtin.Core;

public class Interop : BaseBuiltinStructDefinition
{
    static Interop()
    {
        Configuration.RegisterDefaultOption("interop-modules",
            "mscorelib.dll",
            "runtime.dll");
    }

    public Interop()
        : base("interop")
    {
        DefineBuiltinFunction("get_native_function", true, ExecuteNativeCall);
        DefineBuiltinFunction("quit", true, ExecuteQuitCall);
        DefineBuiltinFunction("enable_option", true, EnableInterpreterOptionCall);
        DefineBuiltinFunction("module_name", true, GetExecutingModuleCall);
        DefineBuiltinFunction("time", true, GetTimeValue);
        DefineBuiltinFunction("get_native_types", true, ExecuteGetNativeTypesCall);
    }

    private static ReturnValue GetTimeValue(Interpreter interpreter, IStruct self, List<Statement> statements)
    {
        if (statements.Count == 0)
        {
            interpreter.Panic("expected an argument signifying the time value to fetch");
        }

        var first = statements.First().Take(interpreter);
        if (first is not long Code)
        {
            interpreter.Panic("the signifying code must be an integer");
            return new ReturnValue(0, 0);
        }

        var now = DateTime.Now;

        return Code switch
        {
            0 => new ReturnValue(now.Millisecond, 0),
            1 => new ReturnValue(now.Second, 0),
            2 => new ReturnValue(now.Minute, 0),
            3 => new ReturnValue(now.Hour, 0),
            4 => new ReturnValue(now.Day, 0),
            _ => new ReturnValue(0, 0),
        };
    }

    private static ReturnValue GetExecutingModuleCall(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        return new ReturnValue(interpreter.Identity, 0);
    }

    private static ReturnValue EnableInterpreterOptionCall(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        foreach (var val in args.Select(stmt => stmt.Take(interpreter)))
        {
            if (val is not string s)
            {
                interpreter.Panic("Interpreter flags can only be strings.");
                return ReturnValue.Bad;
            }

            Configuration.SetFlag(s, true);
        }

        return new ReturnValue(args.Count, 0);
    }

    private static ReturnValue ExecuteQuitCall(Interpreter interpreter, IStruct self, List<Statement> arguments)
    {
        if (arguments.Count == 0)
        {
            Environment.Exit(0);
            return new ReturnValue("unreachable", -1);
        }

        var code = arguments[0].Take(interpreter);
        if (!code.IsIntegral())
        {
            interpreter.Panic($"quit expected an integer, argument. (got '{code}')");
        }

        Environment.Exit(Convert.ToInt32(code));
        return new ReturnValue("unreachable", -1);
    }

    public override string Name => "Lang";
    private static Type[]? _assemblyTypes;

    private static ReturnValue ExecuteGetNativeTypesCall(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        _assemblyTypes ??= LoadAllTypes(interpreter);
        return new(_assemblyTypes.ToList(), 0);
    }

    private static ReturnValue ExecuteNativeCall(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        // TODO: add configuration options that specify paths of DLL's to load symbols from.
        _assemblyTypes ??= LoadAllTypes(interpreter);

        var classSig = args.First().Take(interpreter) as string;

        var match = _assemblyTypes.Where(x => x.Name == classSig);

        var enumerable = match.ToList();
        if (!enumerable.Any())
        {
            interpreter.Panic($"Cannot interop with class {classSig}, it is not loaded.");
            return new ReturnValue(Interpreter.Undefined, 0);
        }

        var @class = enumerable.First();

        var functionName = args[1].Take(interpreter) as string;
        var methods = @class.GetMethods();

        if (methods.All(x => x.Name != functionName))
        {
            return new ReturnValue($"the class `{classSig}` has no function named `{functionName}`", 0);
        }

        var actualMethod = methods.First(x => x.Name == functionName);

        /*
         * TODO: create a wrapper for a native function that can SOMEHOW call it...
         */

        return new ReturnValue(actualMethod, 0);
    }

    private static Type[] LoadAllTypes(Interpreter i)
    {
        var results = new List<Type>();
        var options = Configuration.GetOption("interop-modules")
                      ?? throw new NullReferenceException("config flag interop-modules is not set..");

        foreach (var option in options)
        {
            try
            {
                var mod = Assembly.LoadFrom(option);
                results.AddRange(mod.GetTypes());
                i.ModLog($"loaded module '{option}'");
            }
            catch (IOException exception)
            {
                // TODO: handle exceptions, output to log?
                i.ModLog(exception.Message);
            }
        }

        return results.ToArray();
    }
}