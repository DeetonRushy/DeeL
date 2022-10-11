namespace DL.Parser.Errors;

public enum DErrorCode
{
    // default is 1000 to simulate C# style warnings.
    // when displayed, it can be $"DL{Code}" -> "DL1090" ...
    Default = 1000,
    ExpectedIdentifier,
    ExpectedEquals,
    ExpectedValue

    /* add these as more error scenarios are defined. */
}