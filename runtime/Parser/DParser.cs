using Runtime.Interpreting;
using Runtime.Lexer;
using Runtime.Parser.Errors;
using Runtime.Parser.Exceptions;
using Runtime.Parser.Production;
using Runtime.Parser.Production.Conditions;
using Runtime.Parser.Production.Math;
using System.Runtime.CompilerServices;

namespace Runtime.Parser;

public class DParser
{

    private readonly List<string> BuiltinIdentifiers = new()
    {
        "String",
        "Lang",
        "Console",
        "Thread"
    };
    
    private readonly List<DToken> _tokens;
    public readonly DErrorHandler Errors;
    private bool IsAtEnd => Peek().Type == TokenType.Eof;
    private int _current;
    private bool _wasError;

    private bool _panicked;

    private readonly IDictionary<string, TypeHint> _staticHints;
    private TypeHint GetTypeHintFor(string name) 
        => _staticHints.ContainsKey(name) ? _staticHints[name] : new TypeHint(name);

    public DParser(List<DToken> tokens)
    {
        _tokens = tokens;
        Errors = new DErrorHandler();
        SetErrorLevel(DErrorLevel.All);

        _staticHints = new Dictionary<string, TypeHint>();
    }

    private void SetErrorLevel(DErrorLevel level)
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

            var decl = ParseStatement();

            if (decl is null)
                continue;
            
            Logger.Info(this, $"parsed node at line {decl.Line} [{decl.Debug()}]");
            result.Add(decl);
        }

        return result;
    }

    private FunctionCall ParseFunction(DToken identifier)
    {
        var args = ParseFunctionArguments();
        var call = new FunctionCall(identifier.Lexeme, args.ToArray(), identifier.Line);
        return call;
    }

    private Expression ParseExpression()
    {
        /*
         * FIXME:
         *  Currently, EVERYTHING is parsed from ParseDeclaration.
         *  This makes no sense, so overtime, in places where only an expression
         *  is expected, ParseDeclaration() should be replaced with ParseExpression()
         */
        
        // grouping -- (190 * (92042 + 424))
        if (Match(TokenType.LeftParen))
        {
            return ParseGrouping();
        }

        var primary = ParsePrimary();

        Panic("expected expression");
        return null!;
    }

    public PropertyDeclaration ParseObjectProperty()
    {
        bool isStatic = false;
        _ = Consume(TokenType.Property, "expected `property` keyword.");

        if (Match(TokenType.Star))
        {
            isStatic = true;
            Advance();
        }

        var identifier = Consume(TokenType.Identifier, "expected identifier");
        
        if (!Match(TokenType.Colon))
            Panic($"property `{identifier.Lexeme}` is missing a type annotation");
        Advance();
        if (!Match(TokenType.Identifier))
            Panic($"property `{identifier.Lexeme}` is missing a type annotation");
        var hint = Advance();

        if (!Match(TokenType.Equals))
        {
            return new
                PropertyDeclaration(
                    identifier.Lexeme,
                    isStatic,
                    new TypeHint(hint.Lexeme),
                    null,
                    identifier.Line);
        }

        _ = Advance();
        var initializer = ParseStatement();
        return new PropertyDeclaration(
            identifier.Lexeme,
            isStatic,
            new TypeHint(hint.Lexeme),
            initializer,
            identifier.Line);
    }
    
    private Statement ParseStatement()
    {
        
        // This basically removes the need for ';' at the end of lines.
        // This is intended, line breaks should not be mandatory.
        if (Match(TokenType.LineBreak))
            _ = Consume(TokenType.LineBreak, "");

        if (Match(
                TokenType.Let)
           )
        {
            return ParseLetStatement();
        }

        if (Match(TokenType.Property))
        {
            return ParseObjectProperty();
        }

        if (Match(TokenType.From))
        {
            return ParseSpecificModuleImport();
        }

        if (Match(TokenType.ForcedBreakPoint))
        {
            var @break = Consume(TokenType.ForcedBreakPoint, DErrorCode.Default);
            _ = Consume(TokenType.LineBreak, DErrorCode.ExpLineBreak);
            return new ExplicitBreakpoint(@break.Line);
        }

        // grouping -- (190 * (92042 + 424))
        if (Match(TokenType.LeftParen))
        {
            // TODO: remove this whole case once ParseExpression is used globally instead.
            return ParseGrouping();
        }

        if (Match(TokenType.Return))
        {
            // the next token IS a return statement, no error code
            _ = Consume(TokenType.Return, DErrorCode.Default);
            // return could be used to just return..
            var value = ParseStatement();
            return new ReturnValue(value, value.Line);
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

        if (Match(TokenType.Object))
        {
            return ParseObjectDeclaration();
        }

        if (Match(TokenType.ListOpen))
        {
            var list = ParseListDeclaration();
            return list;
        }

        if (Match(TokenType.LeftBrace))
        {
            var dict = ParseDictDeclaration();
            return dict;
        }

        var primary = ParsePrimary();

        if (Match(TokenType.Equals))
        {
            _ = Consume(TokenType.Equals, "");
            // assignment to an already existing variable.
            var assignmentValue = ParseStatement();

            if (primary is Variable @var)
                return new Assignment(@var, assignmentValue);
        }

        if (primary is Literal literal)
        {
            if (literal.Object is not string identifier)
                goto Skip;

            if (Match(TokenType.Identifier))
            {
                if (Peek().Type is TokenType.InstanceAccess or TokenType.StaticAccess)
                {
                    return ParseVariableAccess();
                }

                if (Peek().Type == TokenType.LeftParen)
                {
                    var args = ParseFunctionArguments();

                    // in this context its okay to consume the linebreak I suppose?
                    // I feel all right-hand assignees should be parsed separate.
                    var @break = Consume(TokenType.LineBreak, "Expected newline after function call");
                    var call = new FunctionCall(identifier, args.ToArray(), @break.Line);

                    return call;
                }

                if (Peek().Type != TokenType.Equals)
                {
                    Panic($"expected `=` after `{identifier}`");
                }
                var eq = Consume(TokenType.Equals, "");
                var lit = ParseStatement();
                if (lit is Variable var)
                    return new Assignment(new Variable(identifier, TypeHint.Any, eq.Line, var.IsConstant), var);
                return new Assignment(new Variable(identifier, TypeHint.Any, eq.Line, false), lit);

            }
        }

        // ReSharper disable once BadControlBracesIndent
        Skip:

        return primary;
    }

    /// <summary>
    /// Handle loading specific members from a module.
    /// </summary>
    /// <returns></returns>
    private Statement ParseSpecificModuleImport()
    {
        _ = Consume(TokenType.From, "expected `from`");
        var mod = Consume(TokenType.String, "expected filename to import.");

        _ = Consume(TokenType.Import, "expected `import` after module name");
        _ = Consume(TokenType.LeftBrace, "expected `{` after `import`");

        if (Match(TokenType.Star))
        {
            // wildcard to import all.
            _ = Advance();
            _ = Consume(TokenType.RightBrace, "importing `*` will cause all definitions to be loaded.\n" + 
                                              "there is no point specifying other definitions while a `*` is present.");

            return new ModuleImport(mod.Lexeme, new[] { "*" }, mod.Line);
        }
        
        // parse identifiers until a right brace.
        var identifiers = new List<string>();

        do
        {
            var tok = Consume(TokenType.Identifier, "expected identifier");
            identifiers.Add(tok.Lexeme);
        } while (MatchAndAdvance(TokenType.Comma));

        _ = Consume(TokenType.RightBrace, "expected `}` to signify the end of the imports");
        return new ModuleImport(mod.Lexeme, identifiers.ToArray(), mod.Line);
    }

    private Statement ParseLetStatement()
    {
        _ = Consume(TokenType.Let, DErrorCode.Default);

        bool isConst = Match(TokenType.Const);
        if (isConst) _ = Advance();

        var identifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);
        var variableName = identifier.Lexeme;

        var hint = TypeHint.Any;

        if (Match(TokenType.Colon))
        {
            var colon = Advance();
            if (!Match(TokenType.Identifier))
            {
                Errors.CreateWithMessage(colon, "expected a type annotation after ':'", true);
            }
            else
            {
                hint = new TypeHint(Advance().Lexeme);
                _staticHints[identifier.Lexeme] = hint;
            }
        }

        var equalSymbol = Consume(TokenType.Equals, DErrorCode.ExpEquals);
        var variable = new Variable(identifier.Lexeme, hint, identifier.Line, isConst);

        if (Match(TokenType.Import))
        {
            // Panic("You can only assign to a non-type hinted variable with an import.");
            var lhs = new Variable(identifier.Lexeme, TypeHint.Any, identifier.Line, isConst);
            return ParseNamespaceImport(lhs);
        }

        if (Match(TokenType.Identifier))
        {
            if (Peek(1).Type is TokenType.InstanceAccess or TokenType.StaticAccess)
            {
                var access = ParseVariableAccess();
                return new Assignment(new Variable(identifier.Lexeme, hint, equalSymbol.Line, isConst), access);
            }

            var rhsIdentifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);

            if (Match(TokenType.Minus, TokenType.Star, TokenType.Plus, TokenType.Divide))
            {
                var mathStatement = HalfParseMathStatement(new Variable(rhsIdentifier.Lexeme, TypeHint.Integer, rhsIdentifier.Line, false));
                return new Assignment(variable, mathStatement);
            }

            if (Peek().Type == TokenType.LeftParen)
            {
                // this needs to return an `Assignment`.
                // The interpreter implementation can then handle actually calling
                // the function.

                var call = ParseFunction(rhsIdentifier);
                var returnType = GetTypeHintFor(call.Identifier);

                if (hint != returnType)
                {
                    Panic($"The call to '{call.Identifier}' returns `{returnType}`. The hint specified is `{hint}`. These do not match.");
                }

                return new Assignment(new(variableName, hint, call.Line), call);
            }

            var contents = rhsIdentifier.Lexeme;

            if (!DVariables.GlobalSymbolExists(contents))
            {
                return new Assignment(new Variable(contents, hint, rhsIdentifier.Line, isConst), new Variable(contents, TypeHint.Any, rhsIdentifier.Line, isConst));
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

            if (hint != predicted)
            {
                Errors.CreateWithMessage(identifier, $"possible type-mismatch - assigning type '{predicted.Name}' to '{hint.Name}'", true);
            }

            return new Assignment(new(variableName, hint, rhsIdentifier.Line), new Literal(tok, hint, inst));
        }

        var value = ParsePrimary();
        if (value is Literal literal && literal.Type != hint)
        {
            Panic($"cannot assign the literal `{literal.Object}` which has the type `{literal.Type}`, to the type `{hint}`");
        }
        return new Assignment(new(variableName, hint, equalSymbol.Line), value);
    }

    private AssignedModuleImport ParseNamespaceImport(Variable assignee)
    {
        _ = Consume(TokenType.Import, "expected 'import'");
        var module = Consume(TokenType.String, "expected module name after `import`");
        return new AssignedModuleImport(assignee, module.Lexeme, module.Line);
    }
    
    private Statement ParseWhileStatement()
    {
        _ = Consume(TokenType.While, string.Empty); // unlikely
        _ = Consume(TokenType.LeftParen, "expected '(' after 'while'");
        var condition = ParseCondition();
        _ = Consume(TokenType.RightParen, "expected ')' after condition");

        var body = ParseBlock();

        return new WhileStatement(condition, body, condition.Line);
    }

    private IfStatement ParseIfStatement()
    {
        _ = Consume(TokenType.If, "expected `if`");
        bool isConst = Match(TokenType.Const);
        if (isConst) _ = Advance();
        _ = Consume(TokenType.LeftParen, "expected `(` after `if`");

        var cond = ParseCondition();

        _ = Consume(TokenType.RightParen, "expected `)`");
        var body = ParseBlock();

        if (!Match(TokenType.Else))
        {
            return new IfStatement(cond, body, null, isConst, cond.Line);
        }

        var @else = Consume(TokenType.Else, string.Empty);
        var fallbackBlock = ParseBlock();

        return new IfStatement(cond, body, fallbackBlock, isConst, @else.Line);
    }

    private Condition ParseCondition()
    {
        var left = ParsePrimary();
        var op = Advance();
        var right = ParsePrimary();

        return op.Type switch
        {
            TokenType.NotEqual => new IsNotEqual(left, right, op.Line),
            TokenType.EqualComparison => new IsEqual(left, right, op.Line),
            _ => throw new NotSupportedException($"the operator '{op.Type}' is not yet implemented.")
        };
    }

    private Statement ParseObjectDeclaration()
    {
        _ = Consume(TokenType.Object, "Expected 'object' keyword");
        var identifier = Consume(TokenType.Identifier, "Expected struct identifier");

        _ = Consume(TokenType.LeftBrace, $"Expected '{{' after {identifier.Lexeme}");
        var declarations = new List<Declaration>();

        while (!Match(TokenType.RightBrace))
        {
            // parse expression when parsing a declaration?
            var next = ParseStatement();
            if (next is not FunctionDeclaration && next is not PropertyDeclaration)
            {
                Panic("declarations within a struct must be a function or property.");
            }
            declarations.Add((Declaration)next);
        }

        // } 
        _ = Consume(TokenType.RightBrace, "Expected right brace after struct declaration");
        _staticHints[identifier.Lexeme] = new TypeHint(identifier.Lexeme);
        return new StructDeclaration(identifier.Lexeme, declarations, identifier.Line);
    }

    private void Panic(string message)
    {
        var current = Peek(-1);
        Errors.CreateWithMessage(current, message, true);
        Errors.DisplayErrors();
        _panicked = true;
        Environment.Exit(-1);
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
            var isConst = Match(TokenType.Const);
            if (isConst)
                _ = Consume(TokenType.Const, "");

            if (Peek(1).Type is TokenType.InstanceAccess or TokenType.StaticAccess)
            {
                var accessExpression = ParseVariableAccess();
                if (!Match(TokenType.Equals))
                    return accessExpression;
                var eq = Consume(TokenType.Equals, $"Expected `=`");
                var rhs = ParseStatement();
                return new VariableAccessAssignment(accessExpression, GetHintFromOperand(rhs), rhs, isConst, eq.Line);
            }


            var rhsIdentifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);

            if (Match(TokenType.Minus, TokenType.Star, TokenType.Plus, TokenType.Divide))
            {
                var mathStatement = HalfParseMathStatement(new Variable(rhsIdentifier.Lexeme, TypeHint.Integer, rhsIdentifier.Line, false));
                return mathStatement;
            }

            if (Peek().Type == TokenType.LeftParen)
            {
                // this needs to return an `Assignment`.
                // The interpreter implementation can then handle actually calling
                // the function.

                var call = ParseFunction(rhsIdentifier);

                return call;
            }

            var contents = rhsIdentifier.Lexeme;

            if (_staticHints.ContainsKey(contents))
                return new Variable(contents, _staticHints[contents], rhsIdentifier.Line, isConst);

            if (BuiltinIdentifiers.Contains(contents))
            {
                _current -= 1;
                return ParseVariableAccess();
            }
            
            Panic($"use of undeclared identifier: `{contents}`");
            return null!;

            /*
             * DVariables have a new re-written token & object instance
             * in order to avoid confusing, drawn-out interpreting.
             * 
             * This will make adding variables from the commandline harder.
             * But it's worth it.
             */

        }

        var literal = ParseLiteral();

        if (Match(TokenType.Minus, TokenType.Star, TokenType.Plus, TokenType.Divide))
        {
            var mathStatement = HalfParseMathStatement(literal);
            return mathStatement;
        }

        return literal;
    }

    private TypeHint GetHintFromOperand(Statement operand)
    {
        if (operand is Declaration decl)
            return decl.Type;
        if (operand is Literal literal)
            return literal.Type;

        return TypeHint.Any;
    }

    // FIXME: ParseMathStatement relies on there being a left and right operand.
    // This causes it to try and consume parens and shit.
    private Grouping ParseGrouping()
    {
        var start = Consume(TokenType.LeftParen, "Expected grouping opener.");
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
        return new Grouping(statements, start.Line);
    }

    private MathStatement ParseMathStatement()
    {
        var left = ParseStatement();
        return HalfParseMathStatement(left);
    }

    // FIXME: debug once this is aids.
    private MathStatement HalfParseMathStatement(Statement left)
    {
        // TODO: this function is too specific. 
        // switch things over to ParseExpression instead of specific parsing here.
        
        var op = Advance();
        if (op.Type is TokenType.RightParen or TokenType.LineBreak)
            return null!;
        Statement? statement;
        if (Match(TokenType.Identifier))
        {
            var identifier = Consume(TokenType.Identifier, DErrorCode.ExpIdentifier);
            if (Match(TokenType.LeftParen))
                statement = ParseFunction(identifier: identifier);
            else
                statement = new Variable(identifier.Lexeme, TypeHint.Any, identifier.Line, false);
        }
        else if (Match(TokenType.LeftParen))
            statement = ParseGrouping();
        else
            statement = ParseLiteral();

        var line = statement.Line;

        return op.Type switch
        {
            TokenType.Plus => new Addition(left, statement, line),
            TokenType.Minus => new Subtraction(left, statement, line),
            TokenType.Star => new Multiplication(left, statement, line),
            TokenType.Divide => new Division(left, statement, line),
            _ => throw new ParserException($"unhandled math operator '{op.Type}'")
        };
    }

    private ModuleIdentity ParseModuleIdentifier()
    {
        var mod = Consume(TokenType.Module, DErrorCode.ExpKeyword);
        var identifier = ParseLiteral();

        if (identifier is Literal id) return new ModuleIdentity(id, mod.Line);
        
        AddParseError(DErrorCode.ExpIdentifier);
        return null!;

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
            if (arg is { } var)
            {
                arguments.Add(var);
                continue;
            }
            break; // no args
        } while (MatchAndAdvance(TokenType.Comma));

        var paren = Consume(TokenType.RightParen, DErrorCode.ExpRightParen);
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
        else
        {
            Errors.CreateWithMessage(paren, "expected type annotation after function arguments.", false);
        }

        // parse the block
        var hint = annotation != null ? new TypeHint(annotation) : TypeHint.Any;
        var body = ParseBlock(hint, identifier.Lexeme);

        _staticHints[identifier.Lexeme] = hint;

        return new FunctionDeclaration(identifier.Lexeme, arguments, body, hint, paren.Line);
    }

    private Block ParseBlock(TypeHint? returnType = null, string? bodyName = null)
    {
        _ = Consume(TokenType.LeftBrace, DErrorCode.ExpLeftBrace);
        var statements = new List<Statement>();

        while (!Match(TokenType.RightBrace))
        {
            var statement = ParseStatement();
            if (statement is not null)
            {
                if (statement is ReturnValue rVal 
                    && returnType is not null
                    && returnType != TypeHint.Any)
                {
                    if (rVal.Value is Declaration decl)
                    {
                        if (decl.Type != returnType)
                        {
                            Panic($"`{bodyName}` hints that it returns `{returnType}`, " +
                                  $"but here it returns `{decl.Type}`..");
                        }
                    }

                    if (rVal.Value is Literal literal)
                    {
                        if (returnType != literal.Type)
                        {
                            Panic($"`{bodyName}` hints that it returns `{returnType}`" + 
                                $", but here it returns `{literal.Type}`..");
                        }
                    }

                    if (rVal.Value is Dict dict)
                    {
                        if (returnType != TypeHint.Dict)
                        {
                            Panic($"`{bodyName}` hints that it returns `{returnType}`" +
                                $", but here it returns `{TypeHint.Dict}`..");
                        }
                    }

                    if (rVal.Value is List)
                    {
                        if (returnType != TypeHint.List)
                        {
                            Panic($"`{bodyName}` hints that it returns `{returnType}`" +
                                $", but here it returns `{TypeHint.List}`..");
                        }
                    }
                }
                statements.Add(statement);
            }
            else
                break;
        }
        _ = Consume(TokenType.RightBrace, DErrorCode.ExpRightBrace);
        return new Block(statements);
    }

    private List ParseListDeclaration()
    {
        bool isConst = Match(TokenType.Const);
        if (isConst) _ = Advance();

        var open = Consume(TokenType.ListOpen, DErrorCode.ExpListOpen);
        var elements = new List<Statement>();

        do
        {
            var literal = ParseStatement();
            if (literal is null)
            {
                continue;
            }
            elements.Add(literal);
        } while (MatchAndAdvance(TokenType.Comma));

        var close = Consume(TokenType.ListClose, DErrorCode.ExpListClose);

        return new List(
            open,
            // hacky fix to sort the above loop adding an extra null element..
            elements.ToArray(),
            close,
            isConst,
            open.Line);
    }

    private Statement ParseDictDeclaration()
    {
        bool isConst = Match(TokenType.Const);
        if (isConst) _ = Advance();
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
                elements.Add(new DictAssignment(key, colon, dict, dict.Line));
                continue;
            }

            if (Match(TokenType.ListOpen))
            {
                var list = ParseListDeclaration();
                elements.Add(new DictAssignment(key, colon, list, list.Line));
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

            elements.Add(new DictAssignment(key, colon, value, value.Line));

        } while (MatchAndAdvance(TokenType.Comma));

        var close = Consume(TokenType.RightBrace, DErrorCode.ExpRightBrace);
        return new Dict(
            open, 
            elements.ToArray(), 
            close,
            isConst,
            close.Line);
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
            return new Variable(contents, TypeHint.Any, value.Line, false);
        }

        if (value.Type == TokenType.Decimal)
        {
            if (value.Literal is decimal dec)
            {
                return new Literal(value, TypeHint.Float, dec);
            }

            // wasn't set in the lexer, attempt to convert it here.

            if (!decimal.TryParse(contents, out decimal dec2))
            {
                throw new
                    ParserException("decimal literal could not be parsed.");
            }

            return new Literal(value, TypeHint.Float, dec2);
        }

        if (value.Type == TokenType.Number)
        {
            if (value.Literal is long l)
            {
                return new Literal(value, TypeHint.Integer, l);
            }

            // wasn't set in the lexer, attempt to convert it here.

            if (!long.TryParse(contents, out long l2))
            {
                throw new
                    ParserException("number literal could not be parsed.");
            }

            return new Literal(value, TypeHint.Integer, l2);
        }

        if (value.Type == TokenType.Boolean)
        {
            if (!bool.TryParse(contents, out bool b))
            {
                throw new
                    ParserException("boolean literal could not be parsed.");
            }

            return new Literal(value, TypeHint.Boolean, b);
        }

        if (value.Type == TokenType.String)
        {
            var str = value.Lexeme;
            return new Literal(value, TypeHint.String, str);
        }

        throw new ParserException($"literal is of type {value.Type}, which has not been implemented in DParser.ParseLiteral()");
    }

    private Variable ParseFunctionArgumentDeclaration()
    {
        // specific for arguments inside a function declaration
        // the only things allowed are identifiers & type annotations.

        bool isConst = Match(TokenType.Const);
        if (isConst) Advance();

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
        _staticHints[identifier.Lexeme] = hint;
        return new Variable(identifier.Lexeme, hint, identifier.Line, isConst);
    }

    private VariableAccess ParseVariableAccess()
    {
        var first = Consume(TokenType.Identifier, "expected identifier");

        List<Statement> allIdentifiers = new()
        {
            new Variable(first.Lexeme, TypeHint.Any, first.Line, false)
        };

        while (Match(TokenType.InstanceAccess, TokenType.StaticAccess))
        {
            var accessor = Match(TokenType.InstanceAccess)
                ? Consume(TokenType.InstanceAccess, "expected `::`")
                : Consume(TokenType.StaticAccess, "expected `::` or `.`");
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
            allIdentifiers.Add(new Variable(next.Lexeme, TypeHint.Any, accessor.Line, false));
        }

        return new VariableAccess(allIdentifiers, first.Line);
    }

    private Statement ParseFunctionArgument()
    {

        if (Match(TokenType.Identifier))
        {
            if (Peek(1).Type == TokenType.InstanceAccess)
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
                return new Variable(contents, TypeHint.Any, identifier.Line, false);
            }

            var (tok, inst) = DVariables.GetValueFor(contents);
            return new Literal(DToken.MakeVar(tok.Type), TypeHint.HintFromTokenType(tok.Type), inst);
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

    private DToken Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }

        Panic(message);
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