using System.Collections;

namespace Runtime.Interpreting.Structs;

public interface IStruct : IScope
{
    string Name { get; }
}

public class UserDefinedStruct : IStruct
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

    public void Populate(IStruct other)
    {
        foreach (var member in other)
        {
            Assign(member.Key, member.Value);
        }
    }

    public string Assign(object key, object value)
    {
        return _structScope.Assign(key, value);
    }

    public object GetValue(object key)
    {
        return _structScope.GetValue(key);
    }

    public override string ToString()
    {
        return $"<struct '{Name}'>";
    }

    public IScope GetScope()
    {
        return this;
    }

    public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
    {
        return _structScope.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
