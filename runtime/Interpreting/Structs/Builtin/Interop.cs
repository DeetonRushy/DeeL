using System.Reflection;
using Runtime.Interpreting.Extensions;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Structs.Builtin;

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

    public override string Name => "CSharp";
    private static Type[]? _assemblyTypes;

    private static ReturnValue ExecuteNativeCall(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        // TODO: add configuration options that specify paths of DLL's to load symbols from.
        _assemblyTypes ??= LoadAllTypes(interpreter);

        var classSig = args.First().Take(interpreter) as string;

        var match = _assemblyTypes.Where(x => x.FullName == classSig);

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