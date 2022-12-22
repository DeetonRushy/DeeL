

using Runtime.Lexer;

namespace Runtime.Parser;

public class TypeHint
{
    public static TypeHint String => new("string") { IsString = true };
    public static TypeHint Boolean => new("bool");
    public static TypeHint Integer => new("int") { IsIntegral = true };
    // floats are decimals
    public static TypeHint Float => new("float") { IsIntegral = true };
    public static TypeHint Dict => new("dict");
    public static TypeHint List => new("list");
    public static TypeHint Func => new("function");
    public static TypeHint Void => new("void");
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
        return type switch
        {
            TokenType.String => String,
            TokenType.Boolean => Boolean,
            TokenType.Number => Integer,
            TokenType.Decimal => Float,
            _ => Any,
        };
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null)
        {
            return false;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}
