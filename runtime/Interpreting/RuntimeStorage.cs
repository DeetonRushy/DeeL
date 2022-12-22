using Runtime.Parser.Production;
using System.Collections;

namespace Runtime.Interpreting;

public interface IScope : IEnumerable<KeyValuePair<object, DeeObject<object>>>
{
    public string Assign(Interpreter interpreter, object key, DeeObject<object> value, Statement? statement = null);
    public object GetValue(object key);

    public IScope GetScope();

    public bool ConstInstance { get; set; }
}

public class RuntimeStorage : IEnumerable<KeyValuePair<object, DeeObject<object>>>, IScope
{
    public static DeeObject<object> Undefined
        => new DeeObject<object>(Interpreter.Undefined) { IsConst = true };

    private IDictionary<object, DeeObject<object>> _storage;
    public string Name { get; }
    public bool ConstInstance { get; set; }

    public RuntimeStorage(string Name, bool IsConstInstance = false)
    {
        _storage = new Dictionary<object, DeeObject<object>>();
        this.Name = Name;
        ConstInstance = IsConstInstance;
    }

    public string Assign(Interpreter interpreter, object key, DeeObject<object>? value, Statement? statement = null)
    {
        if (_storage.ContainsKey(key) && _storage[key].IsConst)
        {
            interpreter.Panic($"`{key}` is a constant variable and cannot be re-assigned.");
            return "<error>";
        }

        if (ConstInstance)
        {
            if (statement is not null)
                interpreter.Panic(statement, $"cannot assign to variable `{key}` in scope `{Name}`, the scope is constant in this context.");
            else
                interpreter.Panic($"cannot assign to variable `{key}` in scope `{Name}`, the scope is constant in this context.");
        }

        _storage[key] = value ?? new DeeObject<object>(Interpreter.Undefined);
        Logger.Info(this, $"Assigned `{key}` to `{value}` (const: {value?.IsConst})");
        return $"{{ {key}: {value} }}";
    }

    public object GetValue(object key)
    {
        return !_storage.ContainsKey(key) ? Undefined : _storage[key].Get();
    }

    public bool Contains(object key)
    {
        return _storage.ContainsKey(key);
    }

    public RuntimeStorage Merge(RuntimeStorage other)
    {
        Logger.Info(this, $"Merging with `{other}`");
        
        var dict = new Dictionary<object, DeeObject<object>>();
        _storage.ToList().ForEach(x => dict.Add(x.Key, x.Value));
        other._storage.ToList().ForEach(x => dict.Add(x.Key, x.Value));

        return new RuntimeStorage(Name) { _storage = dict };
    }

    public void Combine(RuntimeStorage other)
    {
        _storage = Merge(other)._storage;
    }

    public IEnumerator<KeyValuePair<object, DeeObject<object>>> GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IScope GetScope()
    {
        return this;
    }

    public override string ToString()
    {
        return $"<scope '{Name}'>";
    }
}
