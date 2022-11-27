using System.Reflection;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Structs.Builtin;

public class Interop : BaseBuiltinStructDefinition
{
    public Interop()
        : base("interop")
    {
        DefineBuiltinFunction("get_native_function", true, ExecuteNativeCall);
    }

    public override string Name => "interop";
    private static Assembly? _executingAssembly;

    private static ReturnValue ExecuteNativeCall(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        // TODO: add configuration options that specify paths of DLL's to load symbols from.
        
        _executingAssembly ??= Assembly.GetExecutingAssembly();
        
        var classSig = args.First().Take(interpreter) as string;
        var types = _executingAssembly.GetTypes();

        var match = types.Where(x => x.FullName == classSig);

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
}