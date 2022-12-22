using Runtime.Parser.Production;
using System.Collections;

namespace Runtime.Interpreting.Structs;

public interface IStruct : IScope
{
    string Name { get; }
}

public class UserDefinedStruct : IStruct
{
    public string Name { get; }
    private readonly RuntimeStorage _structScope;
    private readonly bool _isStaticInstance;

    public bool ConstInstance { 
        get 
        {
            return _structScope.ConstInstance;
        }
        set
        {
            _structScope.ConstInstance = value;
        }
    }

    public UserDefinedStruct(string identifier, bool isStaticInstance)
    {
        _structScope = new RuntimeStorage(identifier);
        Name = identifier;
        _isStaticInstance = isStaticInstance;
    }

    public void Define(Interpreter interpreter, string identifier, DeeObject<object> value, Statement? statement = null)
    {
        _structScope.Assign(interpreter ,identifier, value, statement);
    }

    public void Populate(Interpreter interpreter, IStruct other, Statement? statement = null)
    {
        foreach (var member in other)
        {
            Assign(interpreter, member.Key, new DeeObject<object>(member.Value), statement);
        }
    }

    public string Assign(Interpreter interpreter, object key, DeeObject<object> value, Statement? statement = null)
    {
        return _structScope.Assign(interpreter, key, value, statement);
    }

    public object GetValue(object key)
    {
        return _structScope.GetValue(key);
    }

    public override string ToString()
    {
        return $"<{(_isStaticInstance ? "static" : "instanceof")} struct '{Name}'>";
    }

    public IScope GetScope()
    {
        return this;
    }

    public IEnumerator<KeyValuePair<object, DeeObject<object>>> GetEnumerator()
    {
        return _structScope.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
