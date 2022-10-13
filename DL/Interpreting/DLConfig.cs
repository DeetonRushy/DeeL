using DL.Interpreting.Api;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace DL.Interpreting;

/// <summary>
/// Base config implementation.
/// </summary>
public class DLConfig : IConfig
{
    public DLConfig()
    {
        Elements = new Dictionary<DValue, DValue>();
    }

    public int Count => Elements.Count;

    public IDictionary<DValue, DValue> Elements { get; }

    public DValue GetElement(DValue value)
    {
        return this[value];
    }

    public bool HasElement(DValue value)
    {
        return Elements.Where(
            x => x.Key.Type == value.Type
                     && x.Key == value).Any();
    }

    public bool TryGetElement(DValue value, [NotNullWhen(true)] out DValue? result)
    {
        try
        {
            result = this[value];
        }
        catch (ArgumentOutOfRangeException)
        {
            result = null;
            return false;
        }

        return true;
    }

    public DValue this[DValue key]
    {
        get
        {
            if (!HasElement(key))
                /* add an easy way to ToString the key */
                throw new IndexOutOfRangeException($"no key `{key}`");
            return Elements[key];
        }
    }

    internal void AddElement(KeyValuePair<DValue, DValue> kvp)
        => Elements.Add(kvp.Key, kvp.Value);
}