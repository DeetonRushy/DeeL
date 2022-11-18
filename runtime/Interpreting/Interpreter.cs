using Runtime.Parser.Errors;
using Runtime.Interpreting.Calls;
using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting;

using Pair = KeyValuePair<object, object>;

// Each interpreter is to be seen as a translation unit.

public class Interpreter : ISyntaxTreeVisitor<object>
{
    public static readonly object Undefined = "undefined";
    public string Identity { get; private set; } = "<anon>";
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

    private readonly CallCenter _calls;
    private readonly RuntimeStorage _storage;

    internal RuntimeStorage Scope() => _storage;

    public Interpreter()
    {
        _calls = new CallCenter();
        _storage = new RuntimeStorage();
    }

    public object Interpret(List<DNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is null)
                throw new InterpreterException("internal: parser problem... null node?");

            var result = node.Take(this);
            
            if (AllowsStdout)
                Console.WriteLine(result);
        }

        return Undefined;
    }

    public object VisitAssignment(Assignment assignment)
    {
        var identifier = assignment.Key.Take(this);
        VerifyNotNull(identifier, "assignment identifier cannot be null");
        var value = assignment.Value.Take(this);
        VerifyNotNull(identifier, "value cannot actually be null");

        return _storage.Assign(identifier, value);
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
        if (!_calls.TryGetDefinition(call.Identifier, out ICallable function))
            return Undefined;
        // An arity of -1 means the function expects no specific number of arguments.
        if (function.Arity != -1 && call.Arguments.Length != function.Arity)
        {
            DisplayErr($"'{call.Identifier}' expects {function.Arity} arguments, but {call.Arguments.Length} were supplied.");
            return Undefined;
        }
        return function.Execute(this, call.Arguments);
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

        return (literal.Sentiment.Type, literal.Object) switch
        {
            (TokenType.String, _) => $"{literal.Sentiment.Lexeme}",
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
        if (!_storage.Contains(identifier))
        {
            DisplayErr($"cannot evaluate '{identifier}', it does not exist.");
            return Undefined;
        }
        return _storage.GetValue(identifier);
    }

    private void VerifyNotNull(object? value, string message)
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
}