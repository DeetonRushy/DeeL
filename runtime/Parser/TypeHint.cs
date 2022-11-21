

using Runtime.Lexer;
using System.Data.Common;

namespace Runtime.Parser;

public class TypeHint
{
    public static TypeHint String => new ("string") { IsString = true };
    public static TypeHint Boolean => new("boolean");
    public static TypeHint Integer => new("integer") { IsIntegral = true };
    public static TypeHint Decimal => new("float") { IsIntegral = true };
    public static TypeHint Dict => new("dict");
    public static TypeHint List => new("list");
    public static TypeHint Func => new("function");
    public static TypeHint Any => new("any");

    public string Name { get; set; }
    public bool IsIntegral { get; set; }
    public bool IsString { get; set; }
    public bool IsBoolean { get; set; }

    public TypeHint(string name)
    {
        Name = name;
    }

    public static bool operator ==(TypeHint a, TypeHint b)
        => a.Name == b.Name;
    public static bool operator !=(TypeHint a, TypeHint b)
        => !(a == b);

    public static TypeHint HintFromTokenType(TokenType type)
    {
        switch (type)
        {
            case TokenType.String:
                return String;
            case TokenType.Boolean: 
                return Boolean;
            case TokenType.Number:
                return Integer; 
            case TokenType.Decimal: 
                return Decimal;
            default:
                return Any;
        }
    }
}
