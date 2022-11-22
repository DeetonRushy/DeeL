using Runtime.Parser.Production;

namespace Runtime.Interpreting.Structs;

public class UserDefinedStruct : Scope
{
    public string Name { get; private set; }
    private readonly RuntimeStorage _structScope;

    public UserDefinedStruct(string identifier)
    {
        _structScope = new RuntimeStorage(identifier);
        Name = identifier;
    }

    public void Define(string Identifier, object? Value)
    {
        _structScope.Assign(Identifier, Value);
    }

    public string Assign(object key, object value)
    {
        return _structScope.Assign(key, value);
    }

    public object GetValue(object key)
    {
        return _structScope.GetValue(key);
    }
}
