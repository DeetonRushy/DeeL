namespace DL.Parser;

public class ThisOrThat<T1, T2>
{
    readonly T1? _this;
    readonly T2? _that;

    public ThisOrThat(T1 @this)
    {
        _this = @this;
    }
    public ThisOrThat(T2 that)
    {
        _that = that;
    }

    public bool HasThis()
        => _this != null;
    public bool HasThat()
        => _that != null;

    public T1 This()
        => _this!;
    public T2 That()
        => _that!;

    public T1 EnsureThis()
        => This();
    public T2 EnsureThat()
        => That();
}