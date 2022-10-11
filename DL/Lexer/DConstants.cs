namespace DL.Lexer;

public static class DConstants
{
    public const char EOF = '\0';

    public const char DictOpen = '{';
    public const char DictClose = '}';

    public const char Comma = ',';
    public new const char Equals = '=';

    public const char ListOpen = '[';
    public const char ListClose = ']';

    public static readonly List<char> StringDelims = new() { '\'', '"' };

    public const char Endline = ';';

    public const char Comment = '#';

    public static readonly Dictionary<string, TokenType> SpecialRhsAssignees = new()
    {
        { "MAX_INT", TokenType.MaxInt },
        { "MIN_INT", TokenType.MinInt },
        { "MAX_DECIMAL", TokenType.MaxDecimal },
        { "MIN_DECIMAL", TokenType.MinDecimal }
    };

    public static TokenType? IsSpecialAssignee(string Assignee)
    {
        return SpecialRhsAssignees?[Assignee] ?? null;
    }
}