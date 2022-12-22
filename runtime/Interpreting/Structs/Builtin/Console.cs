using Runtime.Parser.Production;

namespace Runtime.Interpreting.Structs.Builtin;

public class BuiltinConsoleClass : BaseBuiltinStructDefinition
{
    public override string Name => "Console";

    private bool _disabled;

    public BuiltinConsoleClass()
        : base("Console")
    {
        _disabled = false;
        DefineBuiltinFunction("input", true, ExecuteConsoleInput);
        DefineBuiltinFunction("enable", true, ExecuteEnable);
        DefineBuiltinFunction("disable", true, ExecuteDisable);
        DefineBuiltinFunction("is_disabled", true, ExecuteIsDisabled);
        DefineBuiltinFunction("clear", true, (i, s, a) =>
        {
            Console.Clear();
            return new ReturnValue(0, 0);
        });
    }

    public ReturnValue ExecuteDisable(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        _disabled = true;
        return new ReturnValue(_disabled, 0);
    }

    public ReturnValue ExecuteIsDisabled(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        return new ReturnValue(_disabled, 0);
    }

    public ReturnValue ExecuteEnable(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        if (!_disabled)
            return new ReturnValue(_disabled, 0);
        _disabled = false;
        return new ReturnValue(_disabled, 0);
    }
    
    public ReturnValue ExecuteConsoleInput(Interpreter interpreter, IStruct self, List<Statement> args)
    {
        if (_disabled)
            return new ReturnValue(string.Empty, 0);
        
        if (!interpreter.Allows("stdin") || !interpreter.AllowsStdout)
        {
            interpreter.Panic("cannot get user input while `stdin` and/or `stdout` has been disallowed.");
        }
        
        var prompt = ">> ";

        if (args.Count >= 1 && args[0].Take(interpreter) is string s)
        {
            prompt = s;
        }
        
        Console.Write(prompt);
        var input = Console.ReadLine();
        Console.Write("\n");

        return new ReturnValue(input ?? "Ctrl+Z", 0);
    }
}