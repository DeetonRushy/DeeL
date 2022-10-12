using DL.Lexer;
using DL.Parser.Errors;
using DL.Parser.Production;

namespace DL.Parser;

public class DParser
{
    readonly List<DToken> _tokens;
    readonly DErrorHandler _error;
    private bool _isAtEnd;
    private int _current;

    public DParser(List<DToken> tokens)
    {
        _tokens = tokens;
        _error = new DErrorHandler(DSpan.SourceContents!);
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

        if (ConsumeNormalValue() != null)
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

    private DNode ParseListDeclaration()
    {
        throw new NotImplementedException();
    }

    private DNode ParseDictDeclaration()
    {
        throw new NotImplementedException();
    }

    private DNode ParseLiteral()
    {
        throw new NotImplementedException();
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
                Advance();
                return true;
            }
        }
        return false;
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