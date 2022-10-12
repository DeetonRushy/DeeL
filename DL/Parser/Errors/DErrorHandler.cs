namespace DL.Parser.Errors;

public class DErrorHandler
{
    private readonly IDictionary<DErrorCode, (DErrorLevel, string)> _defaultLevels
        = new Dictionary<DErrorCode, (DErrorLevel, string)>()
        {
            /* define these as you go along. */
            { DErrorCode.Default, (DErrorLevel.All, "this shouldn't happen...") },
            { DErrorCode.ExpectedIdentifier, (DErrorLevel.All, "expected an identifier.") },
            { DErrorCode.ExpectedEquals, (DErrorLevel.All, "expected an assignment.") },
            { DErrorCode.ExpectedValue, (DErrorLevel.All, "expected an identifier next to `=`") },
            { DErrorCode.NonNormalKey, (DErrorLevel.All, "keys must be a string, integer or decimal.") },
            { DErrorCode.ExpectedListOpen, (DErrorLevel.All, "expected an opening '['") },
            { DErrorCode.ExpectedListClose, (DErrorLevel.All, "expected a list closer ']'") }
        };

    private readonly string _contents;

    public DErrorLevel Level { get; set; }
    public List<DError> Errors { get; private set; }

    public DErrorHandler(string source)
    {
        _contents = source;
        Errors = new List<DError>();
    }

    public void CreateDefault(DErrorCode code)
    {
        if (!_defaultLevels.TryGetValue(code, out var defaults))
            throw new 
                NotImplementedException(
                $"please implement DErrorCode.{code} in {nameof(DErrorHandler)}.{nameof(_defaultLevels)}");

        var (level, message) = defaults;

        if (level != Level && level > Level)
        {
            // error is not significant due to error level.
            return;
        }

        // display default message.

        DError error = new ()
        {
            Code = code,
            Message = $"DL{(int)code} {code}: {message}"
        };

        Errors.Add(error);
    }

    public void DisplayErrors()
    {
        Errors.ForEach(x =>
        {
            Console.WriteLine(x.Message);
        });
    }
}