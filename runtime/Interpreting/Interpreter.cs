using Runtime.Parser.Errors;
using Runtime.Interpreting.Calls;
using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;
using System.Runtime.InteropServices.ObjectiveC;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Runtime.Parser.Production.Math;
using Runtime.Interpreting.Extensions;
using Runtime.Interpreting.Structs;
using Runtime.Parser.Production.Conditions;
using System.Reflection;

namespace Runtime.Interpreting;

using Pair = KeyValuePair<object, object>;

// Each interpreter is to be seen as a translation unit.

public class Interpreter : ISyntaxTreeVisitor<object>
{
    public const string Undefined = "undefined";
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

        // initialize all types (other than itself) that inherit from IStruct.
        // then populate the global scope with them.

        Assembly.GetExecutingAssembly().GetTypes().ToList().ForEach(x =>
        {
            if (typeof(IStruct) != x && typeof(UserDefinedStruct) != x && x.IsAssignableTo(typeof(IStruct)))
            {
                var instance = Activator.CreateInstance(x) as IStruct;
                if (instance is not null)
                    _global.Assign(instance.Name, instance);
            }
        });
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
                // cannot assign to a variable with the same name as one in an outer scope.
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

            if (value is IStruct structDecl)
            {
                var scopeId = Guid.NewGuid().ToString();

                if (structDecl.GetValue("construct") is Undefined)
                {
                    var result = new UserDefinedStruct(scopeId);
                    result.Populate(structDecl);
                    return result;
                }

                var @struct = new UserDefinedStruct(scopeId);
                @struct.Populate(structDecl);
                if (structDecl.GetValue("construct") is not IStructFunction constructor)
                    throw new InterpreterException($"invalid instance inside of struct scope..");
                _ = constructor.Execute(this, @struct, call.Arguments.ToList());
                return @struct;
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

            if (arg is VariableAccess access)
            {
                arguments.Add(new Literal(DToken.MakeVar(TokenType.String), access.Take(this)));
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

        throw new InterpreterException($"The variable `{variable.Name}` does not exist in any scope.");
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

    // return the result of the calculation (as long)
    public object VisitGrouping(Grouping grouping)
    {
        long result = 0;

        foreach (var operation in grouping.Statements)
        {
            var res = operation.Take(this);
            if (res is decimal dec) result += (long)dec;
            if (res is long @long) result += @long;
        }

        return result;
    }

    public object VisitAddition(Addition addition)
    {
        var left = GetActualValue(addition.Left);
        var right = GetActualValue(addition.Right);

        if (!left.IsIntegral() || !right.IsIntegral())
        {
            throw new InterpreterException($"cannot perform addition between `{left}` and `{right}`");
        }

        if (left is decimal lDec && right is decimal rDec) return lDec + rDec;
        if (left is decimal lDec1 && right is long rLong) return lDec1 + rLong;
        if (left is long lLong && right is decimal rDec1) return lLong + rDec1;
        if (left is long lLong1 && right is long rLong1) return lLong1 + rLong1;

        throw new NotImplementedException($"Math operation failed with Addition: {addition}");
    }

    public object VisitSubtraction(Subtraction subtraction)
    {
        var left = GetActualValue(subtraction.Left);
        var right = GetActualValue(subtraction.Right);

        if (!left.IsIntegral() || !right.IsIntegral())
        {
            throw new InterpreterException($"cannot perform subtraction between `{left}` and `{right}`");
        }

        if (left is decimal lDec && right is decimal rDec) return lDec - rDec;
        if (left is decimal lDec1 && right is long rLong) return lDec1 - rLong;
        if (left is long lLong && right is decimal rDec1) return lLong - rDec1;
        if (left is long lLong1 && right is long rLong1) return lLong1 - rLong1;

        throw new NotImplementedException($"Math operation failed with Addition: {subtraction}");
    }

    public object VisitMultiplication(Multiplication multiplication)
    {
        var left = GetActualValue(multiplication.Left);
        var right = GetActualValue(multiplication.Right);

        if (!left.IsIntegral() || !right.IsIntegral())
        {
            throw new InterpreterException($"cannot perform multiplaction between `{left}` and `{right}`");
        }

        if (left is decimal lDec && right is decimal rDec) return lDec * rDec;
        if (left is decimal lDec1 && right is long rLong) return lDec1 * rLong;
        if (left is long lLong && right is decimal rDec1) return lLong * rDec1;
        if (left is long lLong1 && right is long rLong1) return lLong1 * rLong1;

        throw new NotImplementedException($"Math operation failed with Addition: {multiplication}");
    }

    public object VisitDivision(Division division)
    {
        var left = GetActualValue(division.Left);
        var right = GetActualValue(division.Right);

        if (!left.IsIntegral() || !right.IsIntegral())
        {
            throw new InterpreterException($"cannot perform division between `{left}` and `{right}`");
        }

        if (left is decimal lDec && right is decimal rDec) return lDec / rDec;
        if (left is decimal lDec1 && right is long rLong) return lDec1 / rLong;
        if (left is long lLong && right is decimal rDec1) return lLong / rDec1;
        if (left is long lLong1 && right is long rLong1) return lLong1 / rLong1;

        throw new NotImplementedException($"Math operation failed with Addition: {division}");
    }

    public object GetActualValue(object value)
    {
        object result = value;

        while (result.IsDLObject())
        {
            if (result is Statement statement)
                result = statement.Take(this);
            break;
        }

        return result;
    }

    public object VisitStructDeclaration(StructDeclaration structDeclaration)
    {
        var declaration = new UserDefinedStruct(structDeclaration.Identifier);

        foreach (var decl in structDeclaration.Declarations)
        {
            if (decl is FunctionDeclaration func)
            {
                var smf = new StructMemberFunction(func.Identifier,
                    func.Body,
                    func.Arguments);
                declaration.Define(smf.Name, smf);
            }
        }

        CurrentScope.Assign(declaration.Name, declaration);
        return declaration;
    }

    public object VisitVariableAccess(VariableAccess variableAccess, out IScope? scope)
    {
        IScope? current = null;
        object? result = null;

        for (int i = 0; i <= variableAccess.Tree.Count; i++)
        {
            object? it;
            bool lastIteration = (i + 1 >= variableAccess.Tree.Count);
            bool firstIteration = (i == 0);

            if (variableAccess.Tree[i] is FunctionCall @call)
            {
                object? val;

                if (firstIteration)
                    val = call.Take(this);
                else
                {
                    var id = current?.GetValue(call.Identifier);
                    if (id is not IStructFunction smf || current is null) 
                    {
                        throw new InterpreterException("extreme confusion");
                    }
                    val = smf.Execute(this, (IStruct)current, call.Arguments.ToList());
                }

                if (val is not IScope _scope)
                {
                    if (lastIteration)
                    {
                        result = val;
                        break;
                    }
                    throw new InterpreterException($"The function `{call.Identifier}` returns a value that is not accessable. (cannot `::` an instance of `{val.GetType()}`)");
                }
                current = _scope;
                continue;
            }

            if (current is null)
            {
                it = variableAccess.Tree[i].Take(this);
                if (it is IScope nextScope)
                    current = nextScope;
                continue;
            }
            else
            {
                if (variableAccess.Tree[i] is not Variable variable)
                    throw new InterpreterException("cannot access a non-scoped type.");
                it = current.GetValue(variable.Name);
            }

            if (!lastIteration && it is IScope s)
            {
                current = s;
                continue;
            }
            break;
        }

        scope = current;
        return result ?? Undefined;
    }

    public object VisitIfStatement(IfStatement conditional)
    {
        var c = conditional.Condition.Take(this);

        if (c is not bool condition)
        {
            throw new InterpreterException("Condition returned a value other than true/false...");
        }

        if (condition)
        {
            conditional.SuccessBlock.Take(this);
        }
        else
        {
            conditional.FallbackBlock?.Take(this);
        }

        return Undefined;
    }

    public object VisitIsEqualsComparison(IsEqual isEqual)
    {
        var left = isEqual.Left.Take(this);
        var right = isEqual.Right.Take(this);

        return left.Equals(right);
    }

    public object VisitIsNotEquals(IsNotEqual isNotEqual)
    {
        var left = isNotEqual.Left.Take(this);
        var right = isNotEqual.Right.Take(this);

        return !left.Equals(right);
    }

    public object VisitWhileLoop(WhileStatement whileStatement)
    {
        bool condition;
        bool firstSpin = true;
        do
        {
            if (whileStatement.Condition.Take(this) is not bool nextResult)
                throw new InterpreterException($"Condition returned non-bool value?");
            condition = nextResult;
            if (!condition)
                break;
            if (firstSpin)
            {
                firstSpin = false;
                continue;
            }

            whileStatement.Body.Take(this);
        }
        while (condition);

        return Undefined;
    }
}