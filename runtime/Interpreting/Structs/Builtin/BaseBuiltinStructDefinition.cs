
using Runtime.Parser.Production;
using System.Collections;

namespace Runtime.Interpreting.Structs.Builtin;

public delegate ReturnValue BuiltinStructFunctionDelegate(
    Interpreter interpreter,
    IStruct self,
    List<Statement> arguments
);

public class BuiltinStructFunctionDefinition : IStructFunction
{
    public BuiltinStructFunctionDefinition(
        string Name,
        bool IsStatic,
        BuiltinStructFunctionDelegate callback
        )
    {
        this.Name = Name;
        this.IsStatic = IsStatic;
        this.callback = callback;
    }

    public string Name { get; }

    public bool IsStatic { get; }
    private readonly BuiltinStructFunctionDelegate callback;

    public ReturnValue Execute(Interpreter interpreter, IStruct instance, List<Statement> args)
    {
        return callback(interpreter, instance, args);
    }

    public override string ToString()
    {
        return Name;
    }
}

public class BaseBuiltinStructDefinition : IStruct
{
    private readonly RuntimeStorage _storage;

    protected BaseBuiltinStructDefinition(string scopeName)
    {
        _storage = new RuntimeStorage(scopeName);
    }

    public void DefineBuiltinFunction(string Name, bool isStatic, BuiltinStructFunctionDelegate callback)
    {
        Assign(Name, new BuiltinStructFunctionDefinition(Name, isStatic, callback));
    }

    public virtual string Name { get; internal set; } = "<internal>";

    public string Assign(object key, object value)
        => _storage.Assign(key, value);

    public object GetValue(object key)
        => _storage.GetValue(key);

    public IScope GetScope()
    {
        return _storage;
    }

    public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
