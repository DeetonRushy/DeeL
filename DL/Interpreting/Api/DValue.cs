namespace DL.Interpreting.Api;

/// <summary>
/// Represents a value from the configuration.
/// </summary>
public class DValue
{
    public object? Instance { get; }
    public DType Type { get; }

    public static implicit operator DValue(string value)
    {
        return new DValue(value, DType.String);
    }
    public static implicit operator DValue(decimal value)
    {
        return new DValue(value, DType.Decimal);
    }
    public static implicit operator DValue(long value)
    {
        return new DValue(value, DType.Number);
    }
    public static implicit operator DValue(bool value)
    {
        return new DValue(value, DType.Number);
    }
    public static implicit operator DValue(List<DValue> list)
    {
        return new DValue(list, DType.List);
    }
    public static implicit operator DValue(Dictionary<DValue, DValue> dict)
    {
        return new DValue(dict, DType.Dict);
    }

    public static implicit operator string(DValue value)
    {
        if (value.Type != DType.String)
            ThrowBadType(value, typeof(string));
        if (value.Instance is string s)
            return s;
        return ThrowBadInit<string>(value);
    }
    public static implicit operator decimal(DValue value)
    {
        if (value.Type != DType.Decimal)
            ThrowBadType(value, typeof(decimal));
        if (value.Instance is decimal d)
            return d;
        return ThrowBadInit<decimal>(value);
    }
    public static implicit operator long(DValue value)
    {
        if (value.Type != DType.Number)
            ThrowBadType(value, typeof(long));
        if (value.Instance is long l)
            return l;
        return ThrowBadInit<long>(value);
    }
    public static implicit operator bool(DValue value)
    {
        if (value.Type != DType.Boolean)
            ThrowBadType(value, typeof(bool));
        if (value.Instance is bool l)
            return l;
        return ThrowBadInit<bool>(value);
    }
    public static implicit operator List<DValue>(DValue value)
    {
        if (value.Type != DType.List)
            ThrowBadType(value, typeof(List<DValue>));
        if (value.Instance is List<DValue> l)
            return l;
        return ThrowBadInit<List<DValue>>(value);
    }
    public static implicit operator Dictionary<DValue, DValue>(DValue value)
    {
        if (value.Type != DType.Dict)
            ThrowBadType(value, typeof(Dictionary<DValue, DValue>));
        if (value.Instance is Dictionary<DValue, DValue> d)
            return d;
        return ThrowBadInit<Dictionary<DValue, DValue>>(value);
    }

    private DValue(object? instance, DType type)
    {
        Instance = instance;
        Type = type;
    }

    /// <summary>
    /// Internal constructor for storing KeyValuePairs.
    /// </summary>
    /// <param name="instance"></param>
    internal DValue(object? instance)
    {
        Instance = instance;
        Type = DType.Unknown;
    }

    private static void ThrowBadType(DValue value, Type t)
    {
        throw new 
            InvalidOperationException($"cannot use a value of DType `{value.Type}` as `{t.Name}`");
    }
    private static T ThrowBadInit<T>(DValue value)
    {
        throw new
            InvalidOperationException($"invalid cast. the object type is correct ({value.Type}), but the instance is not of that type.");
    }

    public override string ToString()
    {
        return Instance?.ToString() ?? "<dead-value>";
    }
}