using Pastel;
using Runtime.Lexer;
using Runtime.Parser.Exceptions;
using System.Drawing;
using System.Text;

namespace Runtime.Parser.Errors;

public class DErrorHandler
{
    internal static List<string> SourceLines { get; set; } = new List<string>();

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

    public DErrorLevel Level { get; set; }
    public List<DError> Errors { get; private set; }

    public DErrorHandler()
    {
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

        DError error = new()
        {
            Code = code,
            Message = $"DL{(int)code} {code}: {message} [line {line}]"
        };

        Errors.Add(error);
    }

    public string CreatePrettyErrorMessage(DToken token, string message, bool skipHighlight = false)
    {
        var relevantContent = SourceLines.Skip(token.Line).FirstOrDefault();
        if (relevantContent is null)
        {
            throw new ParserException($"somehow there is no content at line {token.Line}? (msg: {message})");
        }

        string content = string.Empty;
        if (skipHighlight)
        {
            content = relevantContent;
        }
        else
        {
            var tokens = new DLexer(relevantContent) { MaintainWhitespaceTokens = true }.Lex();
            content = new SyntaxHighlighter(tokens).Output();
        }

        var sb = new StringBuilder();
        sb.AppendLine($"{token.Line} | {content}");
        sb.AppendLine(new string('~', relevantContent.Length + 3));
        sb.Append("ERROR".Pastel(Color.Red));
        sb.Append(':');
        sb.AppendLine($" {message}");

        return sb.ToString();
    }

    public void CreateWithMessage(DToken token, string message, bool skipHighlight)
    {
        var pretty = CreatePrettyErrorMessage(token, message, skipHighlight);
        Errors.Add(new DError() { Code = DErrorCode.Default, Message = pretty });
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
                Message = CreatePrettyErrorMessage(token, message)
            };
        }
        else
        {
            error = new()
            {
                Code = code,
                Message = CreatePrettyErrorMessage(token, message) + $"\n({thrower}:{callingLineNumber})"
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

internal class SyntaxHighlighter
{
    private readonly List<DToken> tokens;

    public SyntaxHighlighter(List<DToken> tokens)
    {
        this.tokens = tokens;
    }

    public string Output()
    {
        // We will take ';' as line breaks for this.
        var sb = new StringBuilder();

        foreach (var token in tokens)
        {
            var highlighted = (token.Type, token.Lexeme, token.Literal) switch
            {
                (TokenType.String, _, _) => Highlight($"'{token.Lexeme}'", Color.Orange),
                (TokenType.Number, _, _) => Highlight($"{token.Lexeme}", Color.GreenYellow),
                (TokenType.Boolean, _, _) => Highlight($"{token.Lexeme}", Color.LightBlue),
                (TokenType.Decimal, _, _) => Highlight($"{token.Lexeme}", Color.GreenYellow),
                (TokenType.LineBreak, _, _) => ";",
                (TokenType.Equals, _, _) => "=",
                (TokenType.Whitespace, _, _) => " ",
                (TokenType.Newline, _, _) => "\n",
                (TokenType.Colon, _, _) => ":",
                (TokenType.LeftBrace, _, _) => "{",
                (TokenType.RightBrace, _, _) => "}",
                (TokenType.ListOpen, _, _) => "[",
                (TokenType.ListClose, _, _) => "]",
                (TokenType.LeftParen, _, _) => "(",
                (TokenType.RightParen, _, _) => ")",
                (TokenType.Comma, _, _) => ",",
                (TokenType.Identifier, _, _) => Highlight($"{token.Lexeme}", Color.LightBlue),
                (TokenType.Module, _, _) => Highlight($"mod", Color.Pink),
                (TokenType.Comment, _, _) => Highlight($"#{token.Lexeme}", Color.Gray),
                (TokenType.Let, _, _) => Highlight("let ", Color.Pink),
                (TokenType.Access, _, _) => "::",
                (TokenType.Plus, _, _) => "+",
                (TokenType.Minus, _, _) => "-",
                (TokenType.Divide, _, _) => "/",
                (TokenType.Star, _, _) => "*",
                _ => token.Lexeme
            };

            sb.Append(highlighted);
        }

        return sb.ToString();
    }

    public string Highlight(string text, Color color)
        => text.Pastel(color);
}