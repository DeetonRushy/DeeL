
using Runtime.Interpreting.Exceptions;
using Runtime.Interpreting.Structs;
using Runtime.Parser.Production;

namespace Runtime.Interpreting;

public class UserDefinedFunction
{
    public Block Body { get; private set; }
    public string Name { get; private set; }
    public List<Variable> ExpectedArguments { get; private set; }

    public UserDefinedFunction(string Name, Block block, List<Variable> arguments)
    {
        this.Name = Name;
        Body = block;
        ExpectedArguments = arguments;
    }

    public ReturnValue Execute(Interpreter interpreter, List<Statement> receivedArguments)
    {
        var prevScope = interpreter.ActiveScope;
        interpreter.ActiveScope = new RuntimeStorage($"<fn {Name}>");

        if (receivedArguments.Count != ExpectedArguments.Count)
        {
            throw new InterpreterException($"function `{Name}` expects {ExpectedArguments.Count} argument(s), but got {receivedArguments.Count}.");
        }

        var scopesToRestoreConstness = new List<IScope>();

        // populate the local scope with the arguments
        for (var i = 0; i < ExpectedArguments.Count; ++i)
        {
            var value = receivedArguments[i].Take(interpreter);
            var current = ExpectedArguments[i];

            if (current.IsConstant || current.IsConst)
            {
                if (value is IStruct @struct)
                {
                    @struct.GetScope().ConstInstance = true;
                    scopesToRestoreConstness.Add(@struct.GetScope());
                }

                interpreter.ActiveScope.Assign(
                    interpreter,
                    current.Name,
                    new DeeObject<object>(value)
                    { IsConst = true }, current);
            }
            else
            {
                interpreter.ActiveScope.Assign(interpreter, ExpectedArguments[i].Name,
                new DeeObject<object>(value) { IsConst = ExpectedArguments[i].IsConstant || ExpectedArguments[i].IsConst });
            }
        }

        var result = Body.Take(interpreter);

        // before terminating this scope, check if the return value only exists
        // within this scope. If so, return whatever it is.
        ReturnValue? returnValue = null;

        if (result is ReturnValue ret)
        {
            if (ret.Value is Statement s)
                returnValue = new ReturnValue(s.Take(interpreter), s.Line);
            else
            {
                // how??? to???? get???? Line?????
                returnValue = new ReturnValue(ret.Value, 0);
            }
        }
        else if (result is Statement s)
        {
            returnValue = new ReturnValue(s.Take(interpreter), s.Line);
        }

        interpreter.ActiveScope = prevScope;
        scopesToRestoreConstness.ForEach(x => x.ConstInstance = false);

        return returnValue ?? new ReturnValue(Literal.Undefined, 0);
    }

    public override string ToString()
    {
        return $"<fn '{Name}'>";
    }
}
