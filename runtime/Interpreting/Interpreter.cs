using Runtime.Parser.Errors;
using Runtime.Interpreting.Calls;
using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;
using System.Runtime.InteropServices.ObjectiveC;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Runtime.Interpreting;

using Pair = KeyValuePair<object, object>;

// Each interpreter is to be seen as a translation unit.

public class Interpreter : ISyntaxTreeVisitor<object>
{
    public static readonly object Undefined = "undefined";
    public string Identity { get; internal set; } = "<anon>";
    public readonly Dictionary<string, bool> ModuleFlags = new()
    {
        { "stdout", true },
        { "stdin", false }
    };

    public bool Allows(string key) => ModuleFlags.ContainsKey(key) && ModuleFlags[key];

    public bool AllowsStdout => Allows("stdout");
    public bool AllowsStdin => Allows("stdin");

    // TODO: make the interpreter more like a runtime. Instead of using 
    // an IConfig. I think this language should be more than that.

    internal readonly CallCenter _calls;
    internal readonly RuntimeStorage _global;

    public RuntimeStorage Globals() => _global;

    // this will only ever be not-null when the current context exists within this scope.
    internal RuntimeStorage? _activeScope;

    internal RuntimeStorage CurrentScope => _activeScope != null ? _activeScope : _global;
    // dont look..
    internal object GetFromEither(object key)
        => _global.Contains(key) 
            ? _global.GetValue(key) 
            : _activeScope != null 
               ? _activeScope.Contains(key) ? _activeScope.GetValue(key) : Undefined
               : Undefined;

    internal RuntimeStorage GlobalScope() => _global;
    internal RuntimeStorage? ActiveScope() => _activeScope;

    public Interpreter()
    {
        _calls = new CallCenter();
        _global = new RuntimeStorage("<global>");
    }

    public object Interpret(List<Statement> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is null)
                throw new InterpreterException("internal: parser problem... null node?");

            _ = node.Take(this);
        }

        return Undefined;
    }

    public object VisitAssignment(Assignment assignment)
    {
        var name = assignment.Variable.Name;
        var value = assignment.Statement.Take(this);

        if (value is ReturnValue @return)
            value = @return.Value;

        if (_activeScope != null && _global.Contains(name))
        {
            if (assignment.Variable.IsInitialization)
            {
                // cannot assign to a variable with the same name as an outer scope.
                throw new InterpreterException($"The variable `{name}` already exists in the scope `{_global.Name}`.");
            }

            return _global.Assign(name, value);
        }

        return _activeScope != null 
            ? _activeScope.Assign(name, value)
            : _global.Assign(name, value);
    }

    public object VisitDict(Dict dict)
    {
        var d = new Dictionary<object, object>();
        foreach (var pair in dict.Members)
        {
            var evaluation = pair.Take(this);

            if (evaluation is not Pair actual)
                throw new InterpreterException($"internal: VisitDictAssignment must return a Pair");
            d.Add(actual.Key, actual.Value);
        }

        return d;
    }

    public object VisitDictAssignment(DictAssignment assignment)
    {
        var key = assignment.Key.Take(this);
        VerifyNotNull(key, "Dictionary key is null");
        var value = assignment.Value.Take(this);
        VerifyNotNull(value, "Dictionary value is null");
        return new Pair(key, value);
    }

    public object VisitFunctionCall(FunctionCall call)
    {
        // see if the call is user-defined.
        if (CurrentScope.Contains(call.Identifier))
        {
            var value = CurrentScope.GetValue(call.Identifier);

            if (value is UserDefinedFunction userFunction)
            {
                // do shit
                return userFunction.Execute(this, call.Arguments.ToList());
            }

            throw new InterpreterException($"The variable `{call.Identifier}` is not callable. (it is of type `{value.GetType().Name}`)");
        }

        if (!_calls.TryGetDefinition(call.Identifier, out ICallable function))
            return Undefined;
        // An arity of -1 means the function expects no specific number of arguments.
        if (function.Arity != -1 && call.Arguments.Length != function.Arity)
        {
            DisplayErr($"'{call.Identifier}' expects {function.Arity} arguments, but {call.Arguments.Length} were supplied.");
            return Undefined;
        }
        var arguments = new List<Literal>();
        foreach (var arg in call.Arguments)
        {
            if (arg is FunctionCall paramCall)
            {
                var result = paramCall.Take(this);
                // FIXME: not all functions may return a string...
                var literal = new Literal(DToken.MakeVar(TokenType.String), result);
                arguments.Add(literal);
            }

            if (arg is Literal paramLiteral)
                arguments.Add(paramLiteral);

            if (arg is Variable variable)
            {
                // builtin functions dont have their own scope.
                var @var = GetFromEither(variable.Name);
                arguments.Add(new Literal(DToken.MakeVar(TokenType.String), @var));
            }
        }
        return function.Execute(this, arguments.ToArray());
    }

    public object VisitList(List list)
    {
        var items = new List<object>();
        foreach (var element in list.Literals)
        {
            if (element is null)
            {
                continue;
            }
            var item = element.Take(this);
            if (item is null)
            {
                items.Add(Undefined);
                continue;
            }
            items.Add(item);
        }

        return items;
    }

    public object VisitLiteral(Literal literal)
    {
        // Must return an actual C# type.

        if (literal.Sentiment.Type == TokenType.String)
        {
            var lexeme = literal.Sentiment.Lexeme;
            return lexeme is not null && lexeme != string.Empty ? literal.Sentiment.Lexeme : literal.Object;
        }

        return (literal.Sentiment.Type, literal.Object) switch
        {
            (TokenType.Number, _) => (long)literal.Object,
            (TokenType.Boolean, _) => (bool)literal.Object,
            (TokenType.Null, _) => Undefined,
            (TokenType.Decimal, _) => (decimal)literal.Object,
            _ => Undefined
        };
    }

    public object VisitModuleIdentity(ModuleIdentity moduleIdentity)
    {
        var identifier = moduleIdentity.ModuleName.Take(this);
        
        if (identifier is not string id)
        {
            DisplayErr($"failed to set module identity to '{identifier}', name must be a string.");
            return Undefined;
        }

        Identity = id;
        return id;
    }

    public object VisitSingleEvaluation(SingleEval evaluation)
    {
        var identifier = VisitLiteral(evaluation.identifier);
        if (!_global.Contains(identifier))
        {
            DisplayErr($"cannot evaluate '{identifier}', it does not exist.");
            return Undefined;
        }
        return _global.GetValue(identifier);
    }

    public object VisitVariable(Variable variable)
    {
        var global = _global;
        var local = _activeScope;

        if (local is not null && local.Contains(variable.Name))
        {
            return local.GetValue(variable.Name);
        }

        if (global.Contains(variable.Name))
        {
            return global.GetValue(variable.Name);
        }

        throw new InterpreterException($"The variable `{variable.Name}` does exist in any scope.");
    }

    public object VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var name = functionDeclaration.Identifier;
        var args = functionDeclaration.Arguments;
        var body = functionDeclaration.Body;

        var udf = new UserDefinedFunction(name, body, args);
        CurrentScope.Assign(name, udf);
        return Undefined;
    }

    public object VisitReturnStatement(ReturnValue returnValue)
    {
        return returnValue.Value ?? Undefined;
    }

    public object VisitBlock(Block block)
    {
        foreach (var statement in block.Statements)
        {
            if (statement is ReturnValue @return) return @return;
            _ = statement.Take(this);
        }

        return Undefined;
    }

    internal void VerifyNotNull(object? value, string message)
    {
        if (value is null)
            throw new InterpreterException(message);
    }

    internal void DisplayErr(string message)
    {
        if (AllowsStdout)
        {
            Console.WriteLine($"ERROR: {message} (in module '{Identity}')");
        }
    }

    internal void ModLog(string message)
    {
        if (AllowsStdout) 
        {
            Console.WriteLine($"({Identity}): {message}");
        }
    }

    public object VisitBreakPoint(ExplicitBreakpoint bp)
    {
        if (!Debugger.Launch())
        {
            ModLog($"__break: failed to launch the debugger");
            return Undefined;
        }
        Debugger.Break();
        return Literal.True;
    }
}