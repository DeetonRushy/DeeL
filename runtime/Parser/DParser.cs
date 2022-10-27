using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;
using Runtime.Lexer;
using Runtime.Parser.Errors;
using Runtime.Parser.Exceptions;
using Runtime.Parser.Production;

namespace Runtime.Parser;

public class DParser
{
    private readonly List<DToken> _tokens;
    public readonly DErrorHandler Errors;
    private bool IsAtEnd => Peek().Type == TokenType.Eof;
    private int _current = 0;
    private bool _wasError = false;

    public DParser(List<DToken> tokens)
    {
        _tokens = tokens;
        Errors = new DErrorHandler(DConstants.Contents);
        SetErrorLevel(DErrorLevel.All);
    }

    public void SetErrorLevel(DErrorLevel level)
    {
        Errors.Level = level;
    }

    public List<DNode> Parse()
    {
        var result = new List<DNode>();

        while (!IsAtEnd && !_wasError)
        {
            var decl = ParseDeclaration();
            result.Add(decl);
        }

        return result;
    }

    /*
     * Consume line breaks ';' within here.
     * 
     * Dictionary's can parse other dictionary's within itself, so if
     * they all require a ';' at the end it would be awful.
     * 
     * Each declaration requires a line break, not literals, lists or dicts.
     */

    public DNode ParseDeclaration()
    {
        // the first node should be a declaration.
        // there is only declarations in DL.

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
                _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
                return new Assignment(literal, list);
            }

            if (Match(TokenType.DictOpen))
            {
                var dict = ParseDictDeclaration();
                _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
                return new Assignment(literal, dict);
            }

            if (Match(TokenType.Identifier))
            {
                var identifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);
                var contents = identifier.Lexeme;

                if (Peek().Type == TokenType.CallOpen)
                {
                    // this needs to return an `Assignment`.
                    // The interpreter implementation can then handle actually calling
                    // the function.

                    var args = ParseFunctionArguments();
                    var call = new FunctionCall(contents, args.ToArray());

                    _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);

                    return new Assignment(literal, call);
                }

                if (!DVariables.GlobalSymbolExists(contents))
                {
                    AddParseError(DErrorCode.UndefinedSymbol);
                    return null!;
                }

                /*
                 * DVariables have a new re-written token & object instance
                 * in order to avoid confusing, drawn-out interpreting.
                 * 
                 * This will make adding variables from the commandline harder.
                 * But it's worth it.
                 */

                var (tok, inst) = DVariables.GetValueFor(contents);

                return new Assignment(literal, new Literal(tok, inst));
            }

            // assume its a normal literal.
            var value = ParseLiteral();
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
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
            var literal = ParseLiteral();
            if (literal is null)
            {
                // TODO: verify last literal wasn't just invalid.
                continue;
            }
            elements.Add(literal);
        } while (MatchAndAdvance(TokenType.Comma));

        var close = Consume(TokenType.ListClose, DErrorCode.ExpListClose);

        return new List(
            open,
            // hacky fix to sort the above loop adding an extra null element..
            elements.ToArray(), 
            close);
    }

    private DNode ParseDictDeclaration()
    {
        var open = Consume(TokenType.DictOpen, DErrorCode.ExpDictOpen);

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

        var contents = value.Lexeme;

        if (value.Type == TokenType.Decimal)
        {
            if (value.Literal is decimal dec)
            {
                return new Literal(value, dec);
            }

            // wasn't set in the lexer, attempt to convert it here.

            if (!decimal.TryParse(contents, out decimal dec2))
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

            if (!long.TryParse(contents, out long l2))
            {
                throw new
                    ParserException("number literal could not be parsed.");
            }

            return new Literal(value, l2);
        }

        if (value.Type == TokenType.Boolean)
        {
            if (!bool.TryParse(contents, out bool b))
            {
                throw new
                    ParserException("boolean literal could not be parsed.");
            }

            return new Literal(value, b);
        }

        if (value.Type == TokenType.String)
        {
            var str = value.Lexeme;
            return new Literal(value, str);
        }

        throw new ParserException($"literal is of type {value.Type}, which has not been implemented in DParser.ParseLiteral()");
    }

    private List<Literal> ParseFunctionArguments()
    {
        _ = Consume(TokenType.CallOpen, DErrorCode.ExpCallOpen);

        var literals = new List<Literal>();

        do
        {
            var arg = ParseLiteral();
            literals.Add(arg);
        } while (Match(TokenType.Comma));

        _ = Consume(TokenType.CallClose, DErrorCode.ExpCallClose);
        return literals;
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

    private DToken ConsumeNormalValue()
    {
        if (!Check(TokenType.String)
            && !Check(TokenType.Number)
            && !Check(TokenType.Decimal)
            && !Check(TokenType.Boolean)
            && !Check(TokenType.Identifier))
        {
            return null!;
        }

        return Advance();
    }

    private void AddParseError(DErrorCode code, [CallerMemberName] string cm = "", [CallerLineNumber] int ln = 0)
    {
        Errors.CreateDefaultWithToken(code, Previous(), cm, ln);
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