using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Interpreting;

public class RuntimeStorage : IEnumerable<KeyValuePair<object, object>>
{
    private IDictionary<object, object> _storage;
    public string Name { get; }

    public RuntimeStorage(string Name)
    {
        _storage = new Dictionary<object, object>();
        this.Name = Name;
    }

    public string Assign(object key, object value)
    {
        _storage[key] = value;
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
