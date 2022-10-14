using System.Security.Cryptography.X509Certificates;

namespace DL.Lexer;

public static class DConstants
{
    public const char EOF = '\0';

    public const char DictOpen =   '{';
    public const char DictClose =  '}';

    public const char Comma =      ',';
    public new const char Equals = '=';

    public const char ListOpen =   '[';
    public const char ListClose =  ']';

    public const char CallOpen =   '(';
    public const char CallClose =  ')';

    public const char Colon =      ':';

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
        = "ABCDEFGHIJKLMNOPQRSTUVWXYZwbcdefghijklmnopqrstuvwxyz";

    public static bool IsDLIdentifierChar(char c)
    {
        return c == '$' || c == '_' || AllowedIdentifierChars.Contains(c);
    }
}