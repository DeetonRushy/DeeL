using System.Security.Cryptography.X509Certificates;

namespace Runtime.Lexer;

public static class DConstants
{
    public static readonly Dictionary<string, TokenType> ReservedKeywords = new()
    {
        { "mod", TokenType.Module },
        { "end", TokenType.End },
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "fn", TokenType.Fn },
        { "return", TokenType.Return },
        { "let", TokenType.Let },
        { "__break", TokenType.ForcedBreakPoint },
        { "object", TokenType.Struct }
    };

    public const char EOF = '\0';

    public const char LeftBrace =   '{';
    public const char RightBrace =  '}';

    public const char Comma =      ',';
    public new const char Equals = '=';

    public const char ListOpen =   '[';
    public const char ListClose =  ']';

    public const char LeftParen =   '(';
    public const char RightParen =  ')';

    public const char Colon =      ':';

    public const string Null = "null";

    public static readonly List<char> StringDelims = new() { '\'', '"' };

    public static bool IsStringDelimeter(char c)
    {
        return StringDelims.Contains(c);
    }

    /// <summary>
    /// Attempt to work this into the source, in a way that would allow this to be changed to '\n'
    /// without many problems.
    /// </summary>
    public const char Endline =        '\n';
    public const char LineBreak =      ';';

    public const char WindowsGarbage = '\r';
    public const char Whitespace =     ' ';

    public const char Comment =        '#';

    public static readonly List<string> BooleanValues = new()
    {
        "true",
        "false"
    };

    public const char Minus = '-';
    public const char Plus = '+';
    public const char GreaterThan = '>';
    public const char LesserThan = '<';
    public const char Multipy = '*';
    public const char Divide = '/';
    public const char Modulo = '%';
    public const char Bang = '!';

    public static bool IsDLNumberCharacter(char ch)
    {
        if (ch <= 31)
        {
            /* anything under character code 31 is stupid shit. */
            return false;
        }

        // temporary fix
        if (ch == ':' || ch == ';')
        {
            return false;
        }

        // It's okay if the numbers looks like this: 1.2.2.34.24.
        // It's invalid & will fail checks with decimal.TryParse.
        return ch >= '0' && ch <= '9' || ch == '.';
    }

    private const string AllowedIdentifierChars
        = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static bool IsDLIdentifierChar(char c)
    {
        return c == '$' || c == '_' || AllowedIdentifierChars.Contains(c);
    }
}