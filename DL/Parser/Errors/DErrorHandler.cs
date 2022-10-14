using DL.Lexer;

namespace DL.Parser.Errors;

public class DErrorHandler
{
    private readonly IDictionary<DErrorCode, (DErrorLevel, string)> _defaultLevels
        = new Dictionary<DErrorCode, (DErrorLevel, string)>()
        {
            /* define these as you go along. */
            { DErrorCode.Default, (DErrorLevel.All, "this shouldn't happen...") },
            { DErrorCode.ExpIdentifier, (DErrorLevel.All, "expected an identifier.") },
            { DErrorCode.ExpEquals, (DErrorLevel.All, "expected an assignment.") },
            { DErrorCode.ExpValue, (DErrorLevel.All, "expected an identifier next to `=`") },
            { DErrorCode.InvalidKey, (DErrorLevel.All, "keys must be a string, integer or decimal.") },
            { DErrorCode.ExpListOpen, (DErrorLevel.All, "expected an opening '['") },
            { DErrorCode.ExpListClose, (DErrorLevel.All, "expected a list closer ']'") },
            { DErrorCode.ExpDictOpen, (DErrorLevel.All, "expected a dict opening '{'") },
            { DErrorCode.ExpDictClose, (DErrorLevel.All, "expected a dict closing '}'") },
            { DErrorCode.ExpColonDictPair, (DErrorLevel.All, "expected a colon ':', between a dictionary pair") },
            { DErrorCode.ExpDictValue, (DErrorLevel.All, "expected a value inside of dictionary pair.") },
            { DErrorCode.ExpLineBreak, (DErrorLevel.All, "expected a ';' at the end of a declaration or statement.") },
            { DErrorCode.UndefinedSymbol, (DErrorLevel.All, "undefined symbol `{0}`") },
            { DErrorCode.ExpCallOpen, (DErrorLevel.Many, "expected a '('") },
            { DErrorCode.ExpCallClose, (DErrorLevel.Many, "expected a ')'") }
        };

    private readonly string _contents;

    public DErrorLevel Level { get; set; }
    public List<DError> Errors { get; private set; }

    public DErrorHandler(string source)
    {
        _contents = source;
        Errors = new List<DError>();
    }

    public void CreateDefault(DErrorCode code, int line)
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
            Message = $"DL{(int)code} {code}: {message} [line {line}]"
        };

        Errors.Add(error);
    }

    public void CreateDefaultWithToken(DErrorCode code, DToken token, string thrower, int callingLineNumber)
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

        DError error;

        if (message.Contains('{') && message.Contains('}'))
        {
            error = new()
            {
                Code = code,
                Message = string.Format(
                    $"DL{(int)code} {code}: {message} [line {token.Line}]\n (originates from {thrower}:{callingLineNumber})",
                    token.Lexeme
                    )
            };
        }
        else
        {
            error = new()
            {
                Code = code,
                Message = $"DL{(int)code} {code}: {message} [line {token.Line}]\n (originates from {thrower}:{callingLineNumber})"
            };
        }

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