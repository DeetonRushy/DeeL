
using Runtime.Interpreting.Exceptions;
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

        // populate the local scope with the arguments
        for (int i = 0; i < ExpectedArguments.Count; ++i)
        {
            var value = receivedArguments[i].Take(interpreter);
            interpreter.ActiveScope.Assign(ExpectedArguments[i].Name, value);
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

        return returnValue ?? new ReturnValue(Literal.Undefined, 0);
    }

    public override string ToString()
    {
        return $"<fn '{Name}'>";
    }
}
