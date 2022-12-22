

namespace Runtime.Interpreting;

public class DeeObject<T>
{
    private T _value;
    public DeeObject(T value)
    {
        this._value = value;
    }

    public T Get() { return _value; }
    public void Set(T value) { _value = value; }

    public bool IsConst { get; set; }

    public override string ToString()
    {
        return $"{_value}";
    }
}
