using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;
using Runtime.Lexer;
using Runtime.Parser.Errors;
using Runtime.Parser.Exceptions;
using Runtime.Parser.Production;
using Runtime.Interpreting;
using Runtime.Parser.Production.Math;
using Runtime.Parser.Production.Conditions;

namespace Runtime.Parser;

public class DParser
{
    private readonly List<DToken> _tokens;
    public readonly DErrorHandler Errors;
    private bool IsAtEnd => Peek().Type == TokenType.Eof;
    private int _current = 0;
    private bool _wasError = false;
    private List<string> _source;

    private bool _panicked = false;

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
            if (_panicked)
                break;

            if (Match(TokenType.Comment))
            {
                Advance();
                continue;
            }

            var decl = ParseExpression();

            if (decl is null)
                continue;

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

    public Statement ParseExpression()
    {
        if (Match(TokenType.LineBreak))
            _ = Consume(TokenType.LineBreak, "");

        if (Match(
            TokenType.Let)
            )
        {
            return ParseLetStatement();
        }

        // parse top level function calls

        if (Match(TokenType.ForcedBreakPoint))
        {
            _ = Consume(TokenType.ForcedBreakPoint, DErrorCode.Default);
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            return new ExplicitBreakpoint();
        }

        // grouping -- (190 * (92042 + 424))
        if (Match(TokenType.LeftParen))
        {
            return ParseGrouping();
        }

        if (Match(TokenType.Return))
        {
            // the next token IS a return statement, no error code
            _ = Consume(TokenType.Return, DErrorCode.Default);
            // return could be used to just return..
            var value = ParseExpression();
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

        if (Match(TokenType.If))
        {
            return ParseIfStatement();
        }

        if (Match(TokenType.While))
        {
            return ParseWhileStatement();
        }

        if (Match(TokenType.Struct))
        {
            return ParseStructDeclaration();
        }

        if (Match(TokenType.ListOpen))
        {
            var list = ParseListDeclaration();
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            return list;
        }

        if (Match(TokenType.LeftBrace))
        {
            var dict = ParseDictDeclaration();
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            return dict;
        }

        var primary = ParsePrimary();

        if (Match(TokenType.Minus, TokenType.Star, TokenType.Plus, TokenType.Divide))
        {
            var mathStatement = HalfParseMathStatement(primary);
            return mathStatement;
        }

        if (Match(TokenType.Equals))
        {
            _ = Consume(TokenType.Equals, "");
            // assignment to an already existing variable.
            var assignmentValue = ParseExpression();

            if (primary is Variable @var)

            return new Assignment(@var, assignmentValue);
        }

        if (primary is Literal literal)
        {
            if (literal.Object is not string identifier)
                goto Skip;

            if (Match(TokenType.Identifier))
            {
                if (Peek().Type == TokenType.Access)
                {
                    return ParseVariableAccess();
                }

                if (Peek().Type == TokenType.LeftParen)
                {
                    var args = ParseFunctionArguments();
                    var call = new FunctionCall(identifier, args.ToArray());

                    // in this context its okay to consume the linebreak I suppose?
                    // I feel all right-hand assignees should be parsed seperate.
                    _ = Consume(TokenType.LineBreak, "Expected newline after function call");

                    return call;
                }

                if (Peek().Type == TokenType.Equals)
                {
                    _ = Consume(TokenType.Equals, "");
                    var lit = ParseExpression();
                    _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
                    if (lit is Variable var)
                        return new Assignment(new(identifier, TypeHint.Any), var);
                    return new Assignment(new(identifier, TypeHint.Any), lit);
                }

                return new Variable(identifier, TypeHint.Any);
            }
        }

    Skip:

        return primary;
    }

    private Statement ParseLetStatement()
    {
        _ = Consume(TokenType.Let, DErrorCode.Default);

        var identifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);
        var variableName = identifier.Lexeme;

        TypeHint hint = TypeHint.Any;

        if (Match(TokenType.Colon))
        {
            var colon = Advance();
            if (!Match(TokenType.Identifier))
            {
                Errors.CreateWithMessage(colon, "expected a type annotation after ':'", true);
            }
            else
                hint = new TypeHint(Advance().Lexeme);
        }

        Consume(TokenType.Equals, DErrorCode.ExpEquals);

        if (Match(TokenType.Identifier))
        {
            if (Peek(1).Type == TokenType.Access)
            {
                var access = ParseVariableAccess();
                return new Assignment(new Variable(identifier.Lexeme, hint), access);
            }

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
                return new Assignment(new Variable(contents, hint), new Variable(contents, TypeHint.Any));
            }

            /*
             * DVariables have a new re-written token & object instance
             * in order to avoid confusing, drawn-out interpreting.
             * 
             * This will make adding variables from the commandline harder.
             * But it's worth it.
             */

            var (tok, inst) = DVariables.GetValueFor(contents);
            var predicted = TypeHint.HintFromTokenType(tok.Type);

            if (hint.Name != "any")
            {
                if (hint != predicted)
                {
                    Errors.CreateWithMessage(identifier, $"possible type-mismatch - assigning type '{predicted.Name}' to '{hint.Name}'", true);
                }
            }
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);

            return new Assignment(new(variableName, hint), new Literal(tok, inst));
        }

        var value = ParsePrimary();
        return new Assignment(new(variableName, hint), value);
    }

    private Statement ParseWhileStatement()
    {
        _ = Consume(TokenType.While, string.Empty); // unlikely
        _ = Consume(TokenType.LeftParen, "expected '(' after 'while'");
        var condition = ParseCondition();
        _ = Consume(TokenType.RightParen, "expected ')' after condition");

        var body = ParseBlock();

        return new WhileStatement(condition, body);
    }

    private IfStatement ParseIfStatement()
    {
        var @if = Consume(TokenType.If, "expected `if`");
        _ = Consume(TokenType.LeftParen, "expected `(` after `if`");

        var cond = ParseCondition();

        _ = Consume(TokenType.RightParen, "expected `)`");
        var body = ParseBlock();

        if (!Match(TokenType.Else))
        {
            return new IfStatement(cond, body, null);
        }

        _ = Consume(TokenType.Else, string.Empty);
        var fallbackBlock = ParseBlock();

        return new IfStatement(cond, body, fallbackBlock);
    }

    private Condition ParseCondition()
    {
        var left = ParsePrimary();
        var op = Advance();
        var right = ParsePrimary();

        return op.Type switch
        {
            TokenType.NotEqual => new IsNotEqual(left, right),
            TokenType.EqualComparison => new IsEqual(left, right),
            _ => throw new NotSupportedException($"the operator '{op.Type}' is not yet implemented.")
        };
    }

    private Statement ParseStructDeclaration()
    {
        _ = Consume(TokenType.Struct, "Expected 'struct' keyword");
        var identifier = Consume(TokenType.Identifier, "Expected struct identifier");

        _ = Consume(TokenType.LeftBrace, $"Expected '{{' after {identifier.Lexeme}");
        var declarations = new List<Declaration>();

        while (!Match(TokenType.RightBrace))
        {
            var next = ParseExpression();
            if (next is not FunctionDeclaration or Assignment)
            {
                Panic("declarations within a struct must be a function or member variable.");
            }
            declarations.Add((Declaration)next);
        }

        // } 
        _ = Consume(TokenType.RightBrace, "Expected right brace after struct declaration");

        return new StructDeclaration(identifier.Lexeme, declarations);
    }

    private void Panic(string message)
    {
        var current = Peek();
        Errors.CreateWithMessage(current, message, true);

    }

    private Statement ParsePrimary()
    {
        if (Match(TokenType.ListOpen))
        {
            var list = ParseListDeclaration();
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            return list;
        }

        if (Match(TokenType.LeftBrace))
        {
            var dict = ParseDictDeclaration();
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            return dict;
        }

        if (Match(TokenType.Identifier))
        {
            if (Peek(1).Type == TokenType.Access)
                return ParseVariableAccess();

            var rhsIdentifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);

            if (Peek().Type == TokenType.LeftParen)
            {
                // this needs to return an `Assignment`.
                // The interpreter implementation can then handle actually calling
                // the function.

                var call = ParseFunction(rhsIdentifier);

                _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);

                return call;
            }

            var contents = rhsIdentifier.Lexeme;

            if (!DVariables.GlobalSymbolExists(contents))
            {
                return new Variable(contents, TypeHint.Any);
            }

            /*
             * DVariables have a new re-written token & object instance
             * in order to avoid confusing, drawn-out interpreting.
             * 
             * This will make adding variables from the commandline harder.
             * But it's worth it.
             */

            return new Variable(contents, TypeHint.Any);
        }

        return ParseLiteral();
    }

    // FIXME: ParseMathStatement relies on there being a left and right operand.
    // This causes it to try and consume parens and shit.
    private Grouping ParseGrouping()
    {
        _ = Consume(TokenType.LeftParen, "Expected grouping opener.");
        var statements = new List<Statement>();

        while (!Match(TokenType.RightParen))
        {
            if (Match(TokenType.LeftParen))
            {
                var grouping = ParseGrouping();
                statements.Add(grouping);
                continue;
            }

            var statement = ParseMathStatement();
            if (statement is null)
                continue;
            statements.Add(statement);
        }

        _ = Consume(TokenType.RightParen, "expected end of grouping.");
        return new Grouping(statements);
    }

    private MathStatement ParseMathStatement()
    {
        var left = ParseExpression();
        return HalfParseMathStatement(left);
    }

    // FIXME: debug once this is aids.
    private MathStatement HalfParseMathStatement(Statement left)
    {
        var op = Advance();
        if (op.Type is TokenType.RightParen or TokenType.LineBreak)
            return null!;
        Statement? statement;
        if (Match(TokenType.Identifier))
        {
            var identifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);
            if (Match(TokenType.LeftParen))
                statement = ParseFunction(identifier: identifier!);
            else
                statement = new Variable(identifier.Lexeme, TypeHint.Any);
        }
        else if (Match(TokenType.LeftParen))
            statement = ParseGrouping();
        else
            statement = ParseLiteral();

        return op.Type switch
        {
            TokenType.Plus => new Addition(left, statement!),
            TokenType.Minus => new Subtraction(left, statement!),
            TokenType.Star => new Multiplication(left, statement!),
            TokenType.Divide => new Division(left, statement!),
            _ => throw new ParserException($"unhandled math operator '{op.Type}'")
        };
    }

    private ModuleIdentity ParseModuleIdentifier()
    {
        _ = Consume(TokenType.Module, DErrorCode.ExpKeyword);
        var identifier = ParseLiteral();

        if (identifier is not Literal Id)
        {
            AddParseError(DErrorCode.ExpIdentifier);
            return null!;
        }

        _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
        return new ModuleIdentity(Id);
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
                Errors.CreateWithMessage(arrow, "expected a type hint.", true); 
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
            var statement = ParseExpression();
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
        var elements = new List<Statement>();

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
            if (ParseLiteral() is not Literal key)
            {
                Panic("dict keys can only be literal values at the moment");
                throw new Exception();
            }
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
            var value = ParsePrimary();

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

    private Statement ParseLiteral()
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
            // FIXME: literal is reserved for literals, not identifiers?? retard
            return new Variable(contents, TypeHint.Any, false);
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
                Errors.CreateWithMessage(colon, "expected a type annotation after ':'", true);
            }
            else
                annotation = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier).Lexeme;
        }

        // FIXME:
        var hint = annotation != null ? new TypeHint(annotation) : TypeHint.Any;
        return new Variable(identifier.Lexeme, hint);
    }

    private VariableAccess ParseVariableAccess()
    {
        List<Statement> allIdentifiers = new()
        {
            new Variable(Consume(TokenType.Identifier, "expected identifier").Lexeme, TypeHint.Any, false)
        };

        while (Match(TokenType.Access))
        {
            _ = Consume(TokenType.Access, "expected accessor");
            var next = Consume(TokenType.Identifier, "expected identifier after '::'");
            if (next is null)
                break;
            if (Match(TokenType.LeftParen))
            {
                // TODO: handle function calls within the chain of instance accesses
                // example: File::open('content.txt')::read_all();
                allIdentifiers.Add(ParseFunction(next));
                continue;
            }
            allIdentifiers.Add(new Variable(next.Lexeme, TypeHint.Any, false));
        }

        return new VariableAccess(allIdentifiers);
    }

    private Statement ParseFunctionArgument()
    {
        if (Match(TokenType.Identifier))
        {
            if (Peek(1).Type == TokenType.Access)
                return ParseVariableAccess();

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

    private DToken Consume(TokenType type, string message, [CallerMemberName] string m = "", [CallerLineNumber] int l = 0)
    {
        if (Check(type))
        {
            return Advance();
        }

        Errors.CreateWithMessage(Peek(), message, true);
        return null!;
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

    private DToken Peek(int amount = 0)
        => _tokens[_current + amount];

    private DToken Previous()
    {
        if (_current - 1 < 0)
            return DToken.Bad;
        return _tokens[_current - 1];
    }
}