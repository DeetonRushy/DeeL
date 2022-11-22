using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins;

internal class AccessFunction : ICallable
{
    // this function is recursive
    public int Arity => -1;

    public string Identifier => "access";

    private Interpreter State = null!;

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        State = interpreter;

        if (args.Length < 1)
        {
            interpreter.DisplayErr($"access expects at least 2 arguments. example: access('variable', 'key', ...keys|...index)");
            return Literal.False;
        }

        var @var = interpreter.VisitLiteral(args[0]);
        var scope = interpreter.GlobalScope();

        if (!scope.Contains(@var))
        {
            interpreter.DisplayErr($"no such variable `{@var}`");
            return Literal.False;
        }

        var value = scope.GetValue(@var);

        if (args.Length == 1)
        {
            // It's a single variable access, lets just return the value.
            // this will throw if the value is NOT a literal. (I.E dict, list)
            return Literal.CreateFromRuntimeType(value);
        }

        var previousOnes = new List<object>() { value };

        for (int i = 1, q = 0; i < args.Length; i++, q++)
        {
            var next = interpreter.VisitLiteral(args[i]);
            var processed = DoAccess(previousOnes[q], next);
            if (!IsManagedType(processed))
                return Literal.CreateFromRuntimeType(processed);
            previousOnes.Add(processed);
        }

        return Literal.False;
    }

    public object DoAccess(object runtimeType, object accessor)
    {
        if (runtimeType is List<object> managedList)
        {
            if (accessor is not long index)
            {
                State.DisplayErr($"cannot access list! index must be a number.");
                return false;
            }
            var managed = managedList[(int)index];
            return managed;
        }

        if (runtimeType is Dictionary<object, object> dictionary)
        {
            if (!dictionary.ContainsKey(accessor))
            {
                State.DisplayErr($"failed to find key matching `{accessor}`!");
                return false;
            }
            var value = dictionary[accessor];
            return value;
        }

        State.DisplayErr($"internal: DoAccess was passed an unmanaged type to index. {runtimeType}");
        return false;
    }

    private bool IsManagedType(object runtimeType)
    {
        return runtimeType is List<object> or Dictionary<object, object>;
    }
}
