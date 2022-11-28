using System.Collections;

namespace Runtime.Interpreting;

public interface IScope : IEnumerable<KeyValuePair<object, object>>
{
    public string Assign(object key, object value);
    public object GetValue(object key);

    public IScope GetScope();
}

public class RuntimeStorage : IEnumerable<KeyValuePair<object, object>>, IScope
{
    private IDictionary<object, object> _storage;
    public string Name { get; }

    public RuntimeStorage(string Name)
    {
        _storage = new Dictionary<object, object>();
        this.Name = Name;
    }

    public string Assign(object key, object? value)
    {
        _storage[key] = value ?? 0;
        return $"{{ {key}: {value} }}";
    }

    public object GetValue(object key)
    {
        if (!_storage.ContainsKey(key))
        {
            return Interpreter.Undefined;
        }
        return _storage[key];
    }

    public bool Contains(object key)
    {
        return _storage.ContainsKey(key);
    }

    public RuntimeStorage Merge(RuntimeStorage other)
    {
        var dict = new Dictionary<object, object>();
        _storage.ToList().ForEach(x => dict.Add(x.Key, x.Value));
        other._storage.ToList().ForEach(x => dict.Add(x.Key, x.Value));

        return new RuntimeStorage(Name) { _storage = dict };
    }

    public void Combine(RuntimeStorage other)
    {
        _storage = Merge(other)._storage;
    }

    public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
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
}
