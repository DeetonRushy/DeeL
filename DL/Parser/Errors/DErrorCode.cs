namespace DL.Parser.Errors;

public enum DErrorCode
{
    // default is 1000 to simulate C# style warnings and errors.
    // when displayed, it can be $"DL{Code}" -> "DL1090" ...
    Default = 1000,

    /// <summary>
    /// When an identifier was expected, but there wasn't one. An example: if the contents are
    /// `=1`, then this should occur. Because an identifier would be expected.
    /// </summary>
    ExpIdentifier,

    /// <summary>
    /// When an equals sign was expected, but there wasn't one. An example: if the contents are
    /// `"value" "something"`, then this should occur. Because assigning (outside of a dictionary),
    /// `=` must be used.
    /// </summary>
    ExpEquals,

    /// <summary>
    /// When a value was expected, but there wasn't one. An example: if the contents are
    /// `"key" = `, then this should occur. Because you cannot assign something to nothing.
    /// </summary>
    ExpValue,

    /// <summary>
    /// When an identifier is an unexpected type. An example: if the contents are
    /// `[1] = "value"`, then this should occur. Because an array cannot be used as an identifier.
    /// The reason it cant, is because there would be no way to interface with it via code.
    /// </summary>
    InvalidKey,

    /// <summary>
    /// When a list opener was expected, but not present. An example: if the contents are
    /// `"array" = 1, 2, 3]` then this should occur.
    /// </summary>
    ExpListOpen,

    /// <summary>
    /// When a list closing was expected, but not present. An example: if the contents are
    /// `"array" = [1, 2, 3` then this should occur.
    /// </summary>
    ExpListClose,

    /// <summary>
    /// When a dictionary opener was expected, but not present.
    /// </summary>
    ExpDictOpen,

    /// <summary>
    /// When a dictionary closing brace was expected, but not present.
    /// </summary>
    ExpDictClose,

    /// <summary>
    /// Expected a colon.
    /// </summary>
    ExpColonDictPair,

    /// <summary>
    /// Expected a value inside of a dictionary pair.
    /// </summary>
    ExpDictValue,

    /* add these as more error scenarios are defined. */
}