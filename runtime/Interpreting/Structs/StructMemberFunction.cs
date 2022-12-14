

using Runtime.Parser.Production;

namespace Runtime.Interpreting.Structs;

public interface IStructFunction
{
    public string Name { get; }
    public bool IsStatic { get; }
    public ReturnValue Execute(Interpreter interpreter, IStruct instance, List<Statement> args);
}

public class StructMemberFunction : IStructFunction
{
    public string Name { get; private set; }
    public bool IsStatic { get; private set; } = true;
    public bool SelfIsConst { get; private set; } = false;
    private readonly Block _body;
    private readonly List<Variable> _expectedArguments;

    public StructMemberFunction(string identifier, Block body, List<Variable> arguments)
    {
        if (arguments.Count != 0)
        {
            if (arguments[0].Name == "self")
            {
                IsStatic = false;
                if (arguments[0].IsConstant)
                    SelfIsConst = true;
            }
        }

        _body = body;
        _expectedArguments = arguments;
        Name = identifier;
    }

    public ReturnValue Execute(Interpreter interpreter, IStruct instance, List<Statement> args)
    {
        var prev = interpreter.ActiveScope;
        interpreter.ActiveScope = new RuntimeStorage(Name);

        if (!IsStatic)
        {
            interpreter.CurrentScope.Assign(interpreter, "self", 
                new DeeObject<object>(instance) { IsConst = SelfIsConst });

            if (args.Count != (_expectedArguments.Count - 1))
            {
                interpreter.Panic($"function `{instance.Name}::{Name}` expects {_expectedArguments.Count - 1} argument(s), but got {args.Count}.");
            }
        }

        // populate function scope, dont assign to self.

        for (var i = 0; i < (IsStatic ? _expectedArguments.Count : _expectedArguments.Count - 1); ++i)
        {
            var current = _expectedArguments[IsStatic ? i : i + 1];
            var value = args[i].Take(interpreter);

            interpreter.CurrentScope.Assign(interpreter, current.Name,
                new DeeObject<object>(value) { IsConst = current.IsConstant });
        }

        var result = _body.Take(interpreter);

        ReturnValue? returnValue = null;
        if (result is ReturnValue @return)
        {
            if (@return.Value is Statement val)
            {
                returnValue = new ReturnValue(val.Take(interpreter), val.Line);
            }
            else
                returnValue = @return;
        }

        interpreter.ActiveScope = prev;
        // how to get line info..
        return returnValue ?? new ReturnValue(Interpreter.Undefined, 0);
    }
}
