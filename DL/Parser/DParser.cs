using DL.Lexer;
using DL.Parser.Errors;
using DL.Parser.Exceptions;
using DL.Parser.Production;

namespace DL.Parser;

public class DParser
{
    readonly List<DToken> _tokens;
    public readonly DErrorHandler _error;
    private bool _isAtEnd => Peek().Type == TokenType.Eof;
    private int _current;

    public DParser(List<DToken> tokens)
    {
        _tokens = tokens;
        _error = new DErrorHandler(DSpan.SourceContents!);
        SetErrorLevel(DErrorLevel.All);
    }

    public void SetErrorLevel(DErrorLevel level)
    {
        _error.Level = level;
    }

    public List<DNode> Parse()
    {
        var result = new List<DNode>();

        while (!_isAtEnd)
        {
            var decl = ParseDeclaration();
            if (decl is not null)
                result.Add(decl);
        }

        return result;
    }

    public DNode ParseDeclaration()
    {
        // the first node should be a declaration.
        // there is only declarations in DL.

        // safety statement to avoid start of file newlines.
        if (MatchAndAdvance(TokenType.Newline))
        { }

        if (Match(
            TokenType.String,
            TokenType.Number, 
            TokenType.Decimal)
            )
        {
            var literal = ParseLiteral();
            Consume(TokenType.Equals, DErrorCode.ExpectedEquals);

            if (Match(TokenType.ListOpen))
            {
                var list = ParseListDeclaration();
                return new Assignment(literal, list);
            }

            if (Match(TokenType.DictOpen))
            {
                var dict = ParseDictDeclaration();
                return new Assignment(literal, dict);
            }

            // assume its a normal literal.
            var value = ParseLiteral();
            return new Assignment(literal, value);
        }

        _error.CreateDefault(DErrorCode.NonNormalKey);
        return null!;
    }

    private List ParseListDeclaration()
    {
        var open = Consume(TokenType.ListOpen, DErrorCode.ExpectedListOpen);
        var elements = new List<Literal>();

        do
        {
            elements.Add(ParseLiteral());
        } while (Check(TokenType.Comma));

        var close = Consume(TokenType.ListClose, DErrorCode.ExpectedListClose);

        return new List(open, elements.ToArray(), close);
    }

    private DNode ParseDictDeclaration()
    {
        throw new NotImplementedException();
    }

    private Literal ParseLiteral()
    {
        // a literal in this context is a string, number or a decimal.

        var value = ConsumeNormalValue();

        if (value is null)
        {
            _error.CreateDefault(DErrorCode.NonNormalKey);
            return null!;
        }

        if (value.Type == TokenType.Decimal)
        {
            if (value.Literal is decimal dec)
            {
                return new Literal(value, dec);
            }

            // wasn't set in the lexer, attempt to convert it here.

            if (!decimal.TryParse(value.Lexeme.Contents(), out decimal dec2))
            {
                throw new 
                    ParserException("decimal literal could not be parsed.");
            }

            return new Literal(value, dec2);
        }

        if (value.Type == TokenType.Number)
        {
            if (value.Literal is long l)
            {
                return new Literal(value, l);
            }

            // wasnt set in the lexer, attempt to convert it here.

            if (!long.TryParse(value.Lexeme.Contents(), out long l2))
            {
                throw new
                    ParserException("number literal could not be parsed.");
            }

            return new Literal(value, l2);
        }

        if (value.Type == TokenType.String)
        {
            var str = value.Lexeme.Contents();
            return new Literal(value, str);
        }

        throw new ParserException($"literal is of type {value.Type}, which has not been implemented in DParser.ParseLiteral()");
    }

    private DToken Consume(TokenType type, DErrorCode code)
    {
        if (Check(type))
        {
            return Advance();
        }

        // throws
        _error.CreateDefault(code);
        return null!;
    }

    public DToken ConsumeNormalValue()
    {
        // deal with any leading newlines

        if (Match(TokenType.Newline))
            _ = Advance();

        if (!Check(TokenType.String)
            && !Check(TokenType.Number)
            && !Check(TokenType.Decimal))
        {
            return null!;
        }

        return Advance();
    }

    private DToken ConsumeMany(DErrorCode code, params TokenType[] types)
    {
        if (Match(types))
        {
            return Advance();
        }

        _error.CreateDefault(code);
        return null!;
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                return true;
            }
        }
        return false;
    }

    private bool MatchAndAdvance(params TokenType[] types)
    {
        bool result = Match(types);
        if (result)
            Advance();
        return result;
    }

    private bool Check(TokenType type)
    {
        if (_isAtEnd)
        {
            return false;
        }
        return Peek().Type == type;
    }

    private DToken Advance()
    {
        if (!_isAtEnd)
        {
            _current++;
        }
        return Previous();
    }

    private DToken Peek()
        => _tokens[_current];

    private DToken Previous()
        => _tokens[_current - 1];


}