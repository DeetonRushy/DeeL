using System.Collections;

namespace Runtime.Interpreting;

public interface Scope
{
    public string Assign(object key, object value);
    public object GetValue(object key);
}

public class RuntimeStorage : IEnumerable<KeyValuePair<object, object>>, Scope
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
            return "undefined";
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

    public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
