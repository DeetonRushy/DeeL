namespace DL.Parser.Errors;

public enum DErrorLevel
{
    /// <summary>
    /// Only critical errors will halt the parser.
    /// </summary>
    Minimum,
    /// <summary>
    /// Critical errors and serious issues will halt the parser.
    /// </summary>
    Many,
    /// <summary>
    /// All errors and warnings will halt the parser. 
    /// </summary>
    All
}
