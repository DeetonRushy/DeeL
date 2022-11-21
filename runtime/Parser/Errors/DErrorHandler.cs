using Runtime.Lexer;
using Runtime.Parser.Exceptions;
using System.Text;

using Pastel;
using System.Drawing;

namespace Runtime.Parser.Errors;

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
            { DErrorCode.ExpLeftBrace, (DErrorLevel.All, "expected an opening '{'") },
            { DErrorCode.ExpRightBrace, (DErrorLevel.All, "expected a closing '}'") },
            { DErrorCode.ExpColonDictPair, (DErrorLevel.All, "expected a colon ':', between a dictionary pair") },
            { DErrorCode.ExpDictValue, (DErrorLevel.All, "expected a value inside of dictionary pair.") },
            { DErrorCode.ExpLineBreak, (DErrorLevel.All, "expected a ';' at the end of a declaration or statement.") },
            { DErrorCode.UndefinedSymbol, (DErrorLevel.All, "undefined symbol `{0}`") },
            { DErrorCode.ExpLeftParen, (DErrorLevel.Many, "expected a '('") },
            { DErrorCode.ExpRightParen, (DErrorLevel.Many, "expected a ')'") },
            { DErrorCode.ExpKeyword, (DErrorLevel.Many, "expected a keyword") },
            { DErrorCode.ExpFnKeyword, (DErrorLevel.All, "expected `fn`") }
        };

    private readonly List<string> _contents;

    public DErrorLevel Level { get; set; }
    public List<DError> Errors { get; private set; }

    public DErrorHandler(List<string> source)
    {
        _contents = source;
        Errors = new List<DError>();
    }

    public void CreateDefault(DErrorCode code, int line = -1)
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

    public void CreateWithMessage(DToken token, string message)
    {
        var relevantContent = _contents.Skip(token.Line).FirstOrDefault();
        if (relevantContent is null)
        {
            throw new ParserException($"somehow there is no content at line {token.Line}?");
        }

        var sb = new StringBuilder();
        sb.AppendLine(relevantContent);
        sb.AppendLine(new string('~', relevantContent.Length));
        sb.Append("ERROR".Pastel(Color.Red));
        sb.Append(':');
        sb.AppendLine($" {message}");

        Errors.Add(new DError() { Code = DErrorCode.Default, Message = sb.ToString() }); 
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
        Console.WriteLine($"finished with {Errors.Count} error(s)");

        Errors.ForEach(x =>
        {
            Console.WriteLine(x.Message);
        });
    }
}