using Runtime.Lexer;

namespace Runtime.Parser.Production;

public record Literal(DToken Sentiment, object Object) : Statement
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
        return Debug();
    }

    public static Literal Undefined => 
        new(DToken.MakeVar(TokenType.Null), "undefined");

    public static Literal True =>
        new(DToken.MakeVar(TokenType.Boolean), "true");

    public static Literal False =>
        new(DToken.MakeVar(TokenType.Boolean), "false");

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
            return new Literal(DToken.MakeVar(TokenType.String), rt);
        }

        if (rtType == typeof(long))
        {
            return new Literal(DToken.MakeVar(TokenType.Number), rt);
        }

        if (rtType == typeof(decimal))
        {
            return new Literal(DToken.MakeVar(TokenType.Decimal), rt);
        }

        if (rtType == typeof(bool))
        {
            return new Literal(DToken.MakeVar(TokenType.Boolean), rt);
        }

        // other types are managed by the interpreter.

        throw new InvalidDataException($"the type `{rtType.Name}` cannot be converted into a literal.");
    }
}