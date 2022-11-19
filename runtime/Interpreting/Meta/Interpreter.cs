using Runtime.Interpreting.Calls;
using Runtime.Parser;
using Runtime.Parser.Production;
using Runtime.Interpreting.Exceptions;

using System.Reflection;
using Runtime.Interpreting;

namespace Runtime.Interpreting.Meta;

// This should be able to convert a script into a type.

public class Interpreter<T> : ISyntaxTreeVisitor<object> where T: new()
{
    readonly CallCenter _calls;
    readonly T _instance;
    readonly PropertyInfo[] _props;

    readonly Interpreter _other;

    public Interpreter()
    {
        _calls = new CallCenter();
        _instance = new T();

        _props = typeof(T).GetProperties();

        _other = new Interpreter();
    }

    public T Interpret(List<DNode> ast)
    {
        foreach (var node in ast)
        {
            var result = Eval(node);

            if (result is true)
                continue;
            else
            {
                Console.WriteLine($"failed to interpret node {node}");
                continue;
            }
        }

        return _instance;
    }

    public object VisitAssignment(Assignment assignment)
    {
        if (assignment.Key.Take(this) is not string id)
        {
            throw new InterpreterException($"cannot identify a C# property name with a non-string value.");
        }

        if (!_props.Any(x => x.Name == id))
        {
            // hmmm..
            return null!;
        }

        var property = _props.Where(x => x.Name == id).First();        
        var value = assignment.Value.Take(this);

        if (value is long && property.PropertyType == typeof(int))
        {
            value = Convert.ToInt32(value);
        }

        if (value is List<object> l)
        {
            value = ProcessValues(l, property.PropertyType.GetGenericArguments().Single());
        }

        try
        {
            property.SetValue(_instance, value);
        }
        catch (Exception exception)
        {
            throw new InterpreterException($"failed to set property `{id}` [{exception.Message}]");
        }

        // No idea what to return here, so true.
        return true;
    }

    // everything under here needs to return an actual c# instance.

    public object VisitDict(Dict dict)
    {
        IDictionary<object, object> instance = new Dictionary<object, object>();

        foreach (var member in dict.Members)
        {
            if (member.Take(this) is KeyValuePair<object, object> pair)
            {
                instance.Add(pair);
            }
        }

        return instance;
    }

    public object VisitDictAssignment(DictAssignment assignment)
    {
        var key = assignment.Key.Take(this);
        var value = assignment.Value.Take(this);

        return new KeyValuePair<object, object>(key, value);
    }

    public object VisitFunctionCall(FunctionCall call)
    {
        if (!_calls.TryGetDefinition(call.Identifier, out var function))
        {
            throw new BadIdentifierException($"there is no function defined with name `{call.Identifier}`");
        }

        throw new NotImplementedException("Fuck Meta!");
    }

    public object VisitList(List list)
    {
        IList<object> instance = new List<object>();

        foreach (var element in list.Literals)
        {
            var part = element.Take(this);
            instance.Add(part);
        }

        return instance;
    }

    public object VisitLiteral(Literal literal)
    {
        return literal.Object;
    }

    internal object ProcessValues(List<object> list, Type? expected)
    {
        // Manually do all this

        if (expected == typeof(string))
        {
            List<string> result = new();
            foreach (var s in list)
                result.Add(s.ToString()!);
            return result;
        }

        return Array.Empty<object>();
    }

    internal object Accept(DNode node)
    => node.Take(this);

    internal object Eval<U>(U node) where U : DNode
        => node.Take(this);

    public object VisitSingleEvaluation(SingleEval evaluation)
    {
        throw new NotImplementedException();
    }

    public object VisitModuleIdentity(ModuleIdentity moduleIdentity)
    {
        throw new NotImplementedException();
    }
}