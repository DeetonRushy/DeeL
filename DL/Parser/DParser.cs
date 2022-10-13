using DL.Lexer;
using DL.Parser.Errors;
using DL.Parser.Exceptions;
using DL.Parser.Production;

namespace DL.Parser;

public class DParser
{
    readonly List<DToken> _tokens;
    public readonly DErrorHandler _error;
    private bool IsAtEnd => Peek().Type == TokenType.Eof;
    private int _current = 0;
    private bool _wasError = false;

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

        while (!IsAtEnd && !_wasError)
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
            Consume(TokenType.Equals, DErrorCode.ExpEquals);

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

        AddParseError(DErrorCode.InvalidKey);
        return null!;
    }

    private List ParseListDeclaration()
    {
        var open = Consume(TokenType.ListOpen, DErrorCode.ExpListOpen);
        var elements = new List<Literal>();

        do
        {
            if (MatchAndAdvance(TokenType.Newline)) 
            {
                // list item is like this:
                /*
                 * 'thing' = [
                 *     'value',
                 *     'value'
                 * ]
                 */
            }
            var literal = ParseLiteral();
            if (literal is null)
            {
                // TODO: verify last literal wasn't just invalid.
                continue;
            }
            elements.Add(literal);
        } while (MatchAndAdvance(TokenType.Comma, TokenType.Newline));

        var close = Consume(TokenType.ListClose, DErrorCode.ExpListClose);
        _ = MatchAndAdvance(TokenType.Newline);

        return new List(
            open,
            // hacky fix to sort the above loop adding an extra null element..
            elements.ToArray(), 
            close);
    }

    private DNode ParseDictDeclaration()
    {
        var open = Consume(TokenType.DictOpen, DErrorCode.ExpDictOpen);

        // in the case of 'dict' = {\n
        _ = MatchAndAdvance(TokenType.Newline);

        List<DictAssignment> elements = new();

        do
        {
            var key = ParseLiteral();
            var colon = Consume(TokenType.Colon, DErrorCode.ExpColonDictPair);

            if (Match(TokenType.DictOpen))
            {
                // recursive, need to be careful about this.
                var dict = ParseDictDeclaration();
                elements.Add(new DictAssignment(key, colon, dict));
                continue;
            }

            if (Match(TokenType.ListOpen))
            {
                var list = ParseListDeclaration();
                elements.Add(new DictAssignment(key, colon, list));
                continue;
            }

            // can only be a literal.
            var value = ParseLiteral();

            if (value is null)
            {
                // there is no value
                AddParseError(DErrorCode.ExpDictValue);
                // attempt to recover, so further errors can be displayed.
                return null!;
            }

            elements.Add(new DictAssignment(key, colon, value));
            ConsumeTrailingNewline();

        } while (MatchAndAdvance(TokenType.Comma));

        var close = Consume(TokenType.DictClose, DErrorCode.ExpDictClose);
        return new Dict(open, elements.ToArray(), close);
    }

    private Literal ParseLiteral()
    {
        // a literal in this context is a string, number or a decimal.

        var value = ConsumeNormalValue();

        if (value is null)
        {
            AddParseError(DErrorCode.InvalidKey);
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

    private void ConsumeTrailingNewline()
    {
        if (Match(TokenType.Newline))
            Advance();
    }

    private DToken Consume(TokenType type, DErrorCode code)
    {
        if (Check(type))
        {
            return Advance();
        }

        // throws
        AddParseError(code);
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

    private void AddParseError(DErrorCode code)
    {
        _error.CreateDefaultWithToken(code, Peek());
        _wasError = true;
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
        if (IsAtEnd)
        {
            return false;
        }
        return Peek().Type == type;
    }

    private DToken Advance()
    {
        if (!IsAtEnd)
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