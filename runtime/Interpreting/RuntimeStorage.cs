using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Interpreting;

internal class RuntimeStorage
{
    private IDictionary<object, object> _storage;

    public RuntimeStorage()
    {
        _storage = new Dictionary<object, object>();
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
}
