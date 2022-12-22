
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

    public bool ConstInstance
    {
        get { return _storage.ConstInstance; }
        set { _storage.ConstInstance = value; }
    }
    
    public Interpreter? Interpreter { get; set; }

    protected BaseBuiltinStructDefinition(string scopeName)
    {
        _storage = new RuntimeStorage(scopeName);
    }

    public void DefineBuiltinFunction(string Name, bool isStatic, BuiltinStructFunctionDelegate callback)
    {
        Assign(Interpreter!, Name,
            new DeeObject<object>(new BuiltinStructFunctionDefinition(Name, isStatic, callback))
            );
    }

    public virtual string Name { get; internal set; } = "<internal>";

    public string Assign(Interpreter interpreter, object key, DeeObject<object> value, Statement? statement = null)
        => _storage.Assign(interpreter, key, value, statement);

    public object GetValue(object key)
        => _storage.GetValue(key);

    public IScope GetScope()
    {
        return _storage;
    }

    public IEnumerator<KeyValuePair<object, DeeObject<object>>> GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        var name = GetType().Name;
        // prefix with '__' to signal that the objects implementation details 
        // are internal.
        return $"__{name}";
    }
}
