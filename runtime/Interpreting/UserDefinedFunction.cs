
using Runtime.Interpreting.Exceptions;
using Runtime.Parser.Production;

namespace Runtime.Interpreting;

public class UserDefinedFunction
{
    public Block Body { get; private set; }
    public string Name { get; private set; }
    public List<Variable> Arguments { get; private set; }

    public UserDefinedFunction(string Name, Block block, List<Variable> arguments)
    {
        this.Name = Name;
        Body = block;
        Arguments = arguments;
    }

    public ReturnValue? Execute(Interpreter interpreter, List<Statement> args)
    {
        var prevScope = interpreter._activeScope;
        interpreter._activeScope = new RuntimeStorage();

        if (args.Count != Arguments.Count)
        {
            throw new InterpreterException($"function `{Name}` expects {Arguments.Count} arguments, but got {args.Count}.");
        }

        // populate the local scope with the arguments
        for(int i = 0; i < Arguments.Count; ++i)
        {
            var value = args[i].Take(interpreter);
            interpreter._activeScope.Assign(Arguments[i].Name, value);
        }

        var result = Body.Take(interpreter);

        interpreter._activeScope = prevScope;

        if (result is not ReturnValue @return)
        {
            return new ReturnValue(Interpreter.Undefined);
        }

        return @return;
    }
}
