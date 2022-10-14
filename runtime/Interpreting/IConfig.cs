using DL.Interpreting.Api;

namespace DL.Interpreting;

/*
 * simple markup for now.
 * 
 * i'd like to implement saving, which may be confusing. So once that's done,
 * this interface can grow.
 */

/// <summary>
/// A way to interface with a DL configuration.
/// </summary>
public interface IConfig
{
    public int Count { get; }
    public IDictionary<DValue, DValue> Elements { get; }
    
    public bool HasElement(DValue value);

    public DValue GetElement(DValue value);
    public bool TryGetElement(DValue value, out DValue? result);

    /* add set once saving is available. */
    public DValue this[DValue key] { get; }
}