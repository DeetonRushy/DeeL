using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record Literal(DToken Sentiment, TypeHint Type, object Object) : Statement(Sentiment.Line)
{
    public override T Take<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitLiteral(this);
    }

    public override string Debug()
    {
        return $"Literal({Sentiment.Type}): {Object}";
    }

    public override string ToString()
    {
        return Object.ToString() ?? "null";
    }

    public static Literal Undefined =>
        new(DToken.MakeVar(TokenType.Null), TypeHint.Any, "undefined");

    public static Literal True =>
        new(DToken.MakeVar(TokenType.Boolean), TypeHint.Boolean, "true");

    public static Literal False =>
        new(DToken.MakeVar(TokenType.Boolean), TypeHint.Boolean, "false");

    public static Literal CreateFromRuntimeType(object rt)
    {
        /*
         * this cannot be done in clean/clear way right now.
         * The goal is to create a Literal from a C# object.
         * This is totally dynamic. 
         */

        var rtType = rt.GetType();

        if (rtType == typeof(string))
        {
            return new Literal(DToken.MakeVar(TokenType.String), TypeHint.String, rt);
        }

        if (rtType == typeof(long))
        {
            return new Literal(DToken.MakeVar(TokenType.Number), TypeHint.Integer, rt);
        }

        if (rtType == typeof(decimal))
        {
            return new Literal(DToken.MakeVar(TokenType.Decimal), TypeHint.Decimal, rt);
        }

        if (rtType == typeof(bool))
        {
            return new Literal(DToken.MakeVar(TokenType.Boolean), TypeHint.Boolean, rt);
        }

        // other types are managed by the interpreter.

        throw new InvalidDataException($"the type `{rtType.Name}` cannot be converted into a literal.");
    }
}