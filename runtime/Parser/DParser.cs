using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;
using Runtime.Lexer;
using Runtime.Parser.Errors;
using Runtime.Parser.Exceptions;
using Runtime.Parser.Production;
using Runtime.Interpreting;

namespace Runtime.Parser;

public class DParser
{
    private readonly List<DToken> _tokens;
    public readonly DErrorHandler Errors;
    private bool IsAtEnd => Peek().Type == TokenType.Eof;
    private int _current = 0;
    private bool _wasError = false;
    private List<string> _source;

    public DParser(List<DToken> tokens, List<string> source)
    {
        _tokens = tokens;
        _source = source;
        Errors = new DErrorHandler(_source);
        SetErrorLevel(DErrorLevel.All);
    }

    public void SetErrorLevel(DErrorLevel level)
    {
        Errors.Level = level;
    }

    public List<Statement> Parse()
    {
        var result = new List<Statement>();

        while (!IsAtEnd && !_wasError)
        {
            if (Match(TokenType.Comment))
            {
                Advance();
                continue;
            }

            var decl = ParseStatement();
            result.Add(decl);
        }

        return result;
    }

    private FunctionCall ParseFunction(DToken identifier)
    {
        var args = ParseFunctionArguments();
        var call = new FunctionCall(identifier.Lexeme, args.ToArray());
        return call;
    }

    /*
     * Consume line breaks ';' within here.
     * 
     * Dictionary's can parse other dictionary's within itself, so if
     * they all require a ';' at the end it would be awful.
     * 
     * Each declaration requires a line break, not literals, lists or dicts.
     */

    public Statement ParseStatement()
    {
        // the first node should be a declaration.
        // there is only declarations in DL.

        if (Match(
            TokenType.Let)
            )
        {
            _ = Consume(TokenType.Let, DErrorCode.Default);

            var identifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);
            var variableName = identifier.Lexeme;

            Consume(TokenType.Equals, DErrorCode.ExpEquals);

            if (Match(TokenType.ListOpen))
            {
                var list = ParseListDeclaration();
                _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
                return new Assignment(new(variableName, TypeHint.List), list);
            }

            if (Match(TokenType.LeftBrace))
            {
                var dict = ParseDictDeclaration();
                _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
                return new Assignment(new(variableName, TypeHint.Dict), dict);
            }

            if (Match(TokenType.Identifier))
            {
                var rhsIdentifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);

                if (Peek().Type == TokenType.LeftParen)
                {
                    // this needs to return an `Assignment`.
                    // The interpreter implementation can then handle actually calling
                    // the function.

                    var call = ParseFunction(rhsIdentifier);

                    _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);

                    return new Assignment(new(variableName, TypeHint.Func), call);
                }

                var contents = rhsIdentifier.Lexeme;

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
                var rt = TypeHint.HintFromTokenType(tok.Type);

                return new Assignment(new(variableName, rt), new Literal(tok, inst));
            }

            // assume its a normal literal.
            var value = ParseLiteral();
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            var assumedType = TypeHint.HintFromTokenType(value.Sentiment.Type);
            return new Assignment(new(variableName,assumedType), value);
        }

        // parse top level function calls

        if (Match(TokenType.ForcedBreakPoint))
        {
            _ = Consume(TokenType.ForcedBreakPoint, DErrorCode.Default);
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            return new ExplicitBreakpoint();
        }

        if (Match(TokenType.Identifier))
        {
            var identifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier).Lexeme;

            if (Peek().Type == TokenType.LeftParen)
            {
                var args = ParseFunctionArguments();
                var call = new FunctionCall(identifier, args.ToArray());

                _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);

                return call;
            }

            if (Peek().Type == TokenType.Equals)
            {
                // assignment to already existing variable.
                _ = Consume(TokenType.Equals, DErrorCode.ExpEquals);

                if (Peek().Type == TokenType.Identifier) 
                {
                    // assigning pre-existing variable to already existing variables value
                    var right = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);
                    _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
                    return new Assignment(new(identifier, TypeHint.Any, false), new Variable(right.Lexeme, TypeHint.HintFromTokenType(right.Type)));
                }

                var lit = ParseLiteral();
                _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
                var rt = TypeHint.HintFromTokenType(lit.Sentiment.Type);
                return new Assignment(new(identifier, rt), lit);
            }

            return new Variable(identifier, TypeHint.Any);
        }

        if (Match(TokenType.Return))
        {
            // the next token IS a return statement, no error code
            _ = Consume(TokenType.Return, DErrorCode.Default);
            var value = ParseStatement();
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            return new ReturnValue(value);
        }

        if (Match(TokenType.Module))
        {
            return ParseModuleIdentifier();
        }

        if (Match(TokenType.Fn))
        {
            return ParseFunctionDeclaration();
        }

        return ParseLiteral();
    }

    private ModuleIdentity ParseModuleIdentifier()
    {
        _ = Consume(TokenType.Module, DErrorCode.ExpKeyword);
        var identifier = ParseLiteral();

        if (identifier is null)
        {
            AddParseError(DErrorCode.ExpIdentifier);
            return null!;
        }

        _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
        return new ModuleIdentity(identifier);
    }

    private FunctionDeclaration ParseFunctionDeclaration()
    {
        _ = Consume(TokenType.Fn, DErrorCode.ExpFnKeyword);
        var identifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);

        _ = Consume(TokenType.LeftParen, DErrorCode.ExpLeftParen);
        var arguments = new List<Variable>();

        do
        {
            var arg = ParseFunctionArgumentDeclaration();
            // only a variable declaration is allowed within a function
            // declarations argument list.
            if (arg is Variable @var)
            {
                arguments.Add(var);
                continue;
            }
            break; // no args
        } while (MatchAndAdvance(TokenType.Comma));

        _ = Consume(TokenType.RightParen, DErrorCode.ExpRightParen);
        // we are now here: fn name(arg1, arg2) <---
        string? annotation = null;

        if (Match(TokenType.Arrow))
        {
            var arrow = Advance(); // consume arrow
            if (!Match(TokenType.Identifier))
            {
                Errors.CreateWithMessage(arrow, "expected a type hint."); 
            }
            else
            {
                annotation = Advance().Lexeme;
            }
        }

        // parse the block
        var body = ParseBlock();
        var hint = annotation != null ? new(annotation) : TypeHint.Any;

        return new FunctionDeclaration(identifier.Lexeme, arguments, body, hint);
    }

    private Block ParseBlock()
    {
        _ = Consume(TokenType.LeftBrace, DErrorCode.ExpLeftBrace);
        var statements = new List<Statement>();

        while (!Match(TokenType.RightBrace))
        {
            var statement = ParseStatement();
            if (statement is not null)
                statements.Add(statement);
            else
                break;
        }
        _ = Consume(TokenType.RightBrace, DErrorCode.ExpRightBrace);
        return new Block(statements);
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

    private Statement ParseDictDeclaration()
    {
        var open = Consume(TokenType.LeftBrace, DErrorCode.ExpLeftBrace);

        List<DictAssignment> elements = new();

        do
        {
            var key = ParseLiteral();
            var colon = Consume(TokenType.Colon, DErrorCode.ExpColonDictPair);

            if (Match(TokenType.LeftBrace))
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

        var close = Consume(TokenType.RightBrace, DErrorCode.ExpRightBrace);
        return new Dict(open, elements.ToArray(), close);
    }

    private Literal ParseLiteral()
    {
        // a literal in this context is a string, number or a decimal.

        var value = ConsumeNormalValue();

        if (value is null)
        {
            return null!;
        }

        var contents = value.Lexeme;

        if (value.Type == TokenType.Identifier)
        {
            return new Literal(value, contents);
        }

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

    private Variable ParseFunctionArgumentDeclaration()
    {
        // specific for arguments inside a function declaration
        // the only things allowed are identifiers & type annotations.

        if (!Match(TokenType.Identifier))
            return null!;

        var identifier = Advance();
        string? annotation = null;

        if (Match(TokenType.Colon))
        {
            var colon = Advance(); // consume colon
            if (!Match(TokenType.Identifier))
            {
                Errors.CreateWithMessage(colon, "expected a type annotation after ':'");
            }
            else
                annotation = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier).Lexeme;
        }

        // FIXME:
        var hint = annotation != null ? new TypeHint(annotation) : TypeHint.Any;
        return new Variable(identifier.Lexeme, hint);
    }

    private Statement ParseFunctionArgument()
    {
        if (Match(TokenType.Identifier))
        {
            var identifier = Advance();

            // check if this is a function call.
            if (Peek().Type == TokenType.LeftParen)
            {
                var call = ParseFunction(identifier);
                return call;
            }

            var contents = identifier.Lexeme;

            if (!DVariables.GlobalSymbolExists(contents))
            {
                return new Variable(contents, TypeHint.Any);
            }

            var (tok, inst) = DVariables.GetValueFor(contents);
            return new Literal(DToken.MakeVar(tok.Type), inst);
        }

        return ParseLiteral();
    }

    private List<Statement> ParseFunctionArguments()
    {
        _ = Consume(TokenType.LeftParen, DErrorCode.ExpLeftParen);

        var literals = new List<Statement>();

        do
        {
            var arg = ParseFunctionArgument();
            if (arg is not null)
            {
                literals.Add(arg);
                continue;
            }
            break;
        } while (MatchAndAdvance(TokenType.Comma));

        _ = Consume(TokenType.RightParen, DErrorCode.ExpRightParen);
        return literals;
    }

    private DToken Consume(TokenType type, DErrorCode code, [CallerMemberName] string m = "", [CallerLineNumber] int l = 0)
    {
        if (Check(type))
        {
            return Advance();
        }

        // throws
        AddParseError(code, m, l);
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
    {
        if (_current - 1 < 0)
            return DToken.Bad;
        return _tokens[_current - 1];
    }
}