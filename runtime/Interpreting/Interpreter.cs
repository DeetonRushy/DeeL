using Runtime.Interpreting.Calls;
using Runtime.Interpreting.Exceptions;
using Runtime.Interpreting.Extensions;
using Runtime.Interpreting.Structs;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Errors;
using Runtime.Parser.Production;
using Runtime.Parser.Production.Conditions;
using Runtime.Parser.Production.Math;
using System.Diagnostics;
using System.Reflection;
using Runtime.Interpreting.Structs.Builtin;

namespace Runtime.Interpreting;

using Pair = KeyValuePair<object, object>;
using Ast = List<Statement>;

// Each interpreter is to be seen as a translation unit.

public enum InterpreterMode
{
    Main,
    Module
}

public class Interpreter : ISyntaxTreeVisitor<object>
{
    /// <summary>
    /// The interpreters error reporter. Use <see cref="Panic"/> to quit on critical errors.
    /// </summary>
    private readonly DErrorHandler _errors;
    
    /// <summary>
    /// The mode that this interpreter instance is in.
    /// </summary>
    public InterpreterMode Mode { get; }
    
    /// <summary>
    /// constant string that represents `undefined`.
    /// </summary>
    public const string Undefined = "undefined";
    
    /// <summary>
    /// The modules identity. This is changed via a `mod` statement.
    /// </summary>
    public string Identity { get; internal set; } = "<anon>";
    
    /// <summary>
    /// Module specific flags. These can and should be dynamic. This concept could disallow
    /// interpreter behaviour where it is not needed and vice-versa.
    /// </summary>
    public readonly Dictionary<string, bool> ModuleFlags = new()
    {
        { "stdout", true },
        { "stdin", true }
    };

    public bool PreventExit { get; set; } = false;
    
    /// <summary>
    /// Helper function to see if <see cref="ModuleFlags"/> has the flag <see cref="key"/> and that it is true.
    /// </summary>
    /// <param name="key">The flag to check</param>
    /// <returns>True if the flag exists and is true, otherwise false.</returns>
    public bool Allows(string key) => ModuleFlags.ContainsKey(key) && ModuleFlags[key];

    /// <summary>
    /// Easy helper to see if stdout is enabled.
    /// </summary>
    public bool AllowsStdout => Allows("stdout");

    // TODO: make the interpreter more like a runtime. Instead of using 
    // an IConfig. I think this language should be more than that.

    /// <summary>
    /// Functions that are builtin. CallCenter handles loading these functions.
    /// </summary>
    private readonly CallCenter _calls;
    
    /// <summary>
    /// This interpreters global scope.
    /// </summary>
    private readonly RuntimeStorage _global;

    public CallCenter GlobalBuiltinFunctions() => _calls;

    public RuntimeStorage Globals() => _global;

    /// <summary>
    /// The current executing scope (when applicable). This will be null when top-level
    /// statements are executing. However, inside a function this scope should be in-use.
    /// </summary>
    internal RuntimeStorage? ActiveScope;

    /// <summary>
    /// Helper. This will return <see cref="_global"/> if <see cref="ActiveScope"/> is null, otherwise it will
    /// return <see cref="ActiveScope"/>.
    /// </summary>
    internal RuntimeStorage CurrentScope => ActiveScope ?? _global;

    /// <summary>
    /// The AST generated by the parser.
    /// </summary>
    private Ast Statements { get; set; }

    public void ReloadAst(Ast ast)
    {
        Statements = ast;
        Position = 0;
    }
    
    /// <summary>
    /// The current position we're at in <see cref="Statements"/>
    /// </summary>
    private int Position { get; set; }

    /// <summary>
    /// Get the value of a value in either the local scope or global scope.
    /// </summary>
    /// <param name="key">The name of the variable</param>
    /// <returns>The variable from either scope. Prioritizing the local scope.</returns>
    internal object GetFromEither(object key)
    {
        if (ActiveScope is null && !Globals().Contains(key))
        {
            return Undefined;
        }
        
        if (ActiveScope is not null)
        {
            return ActiveScope.Contains(key) ? ActiveScope.GetValue(key) : _global.GetValue(key);
        }

        return _global.Contains(key) ? _global.GetValue(key) : Undefined;
    }

    internal RuntimeStorage GlobalScope() => _global;
    private Statement Peek()
        => Statements[Position];

    /// <summary>
    /// Display an error message, including the current line that was executing with the
    /// provided message.
    /// </summary>
    /// <param name="message">The message to display under the line of code that failed.</param>
    internal void Panic(string message)
    {
        _errors.CreateWithMessage(new DToken { Line = Peek().Line }, message, true);
        _errors.DisplayErrors();

        if (!PreventExit)
        {
            Environment.Exit(-1);
        }
    }

    /// <summary>
    /// This constructor initialized the interpreter in <see cref="InterpreterMode.Main"/> mode.
    /// That means that this interpreter is the 'parent' interpreter. It will be the one that
    /// visits other interpreters that are in <see cref="InterpreterMode.Module"/> mode.
    /// </summary>
    /// <param name="ast">The abstract syntax tree generated by the parser</param>
    public Interpreter(Ast ast)
    {
        Mode = InterpreterMode.Main;
        _calls = new CallCenter();
        _global = new RuntimeStorage("<global>");
        Statements = ast;

        // initialize all types (other than itself) that inherit from IStruct.
        // then populate the global scope with them.
        
        // TODO: this needs to be globalized, so other assembly's can plug in. (replace GetExecutingAssembly) 
        Assembly.GetExecutingAssembly().GetTypes().ToList().ForEach(x =>
        {
            if (typeof(IStruct) == x || typeof(UserDefinedStruct) == x ||
                typeof(BaseBuiltinStructDefinition) == x) return;
            if (!x.IsAssignableTo(typeof(IStruct))) return;
            if (Activator.CreateInstance(x) is IStruct instance)
                _global.Assign(this, instance.Name, new DeeObject<object>(instance));
        });

        _errors = new DErrorHandler();
    }

    public Interpreter(string path)
    {
        // Lex, Parse & interpret the code in the file.
        var info = new FileInfo(path);

        // any panic from here will be our error handler.
        var lexer = new DLexer(info);
        var parser = new DParser(lexer.Lex());

        Mode = InterpreterMode.Module;
        _calls = new CallCenter();
        _global = new RuntimeStorage("<global>");
        var previous = DErrorHandler.SourceLines;
        DErrorHandler.SourceLines = File.ReadAllLines(info.FullName).ToList();
        _errors = new DErrorHandler();
        Statements = parser.Parse();

        _ = Interpret();
        DErrorHandler.SourceLines = previous;
    }

    public object Interpret()
    {
        Dictionary<Type, long> productionTimes = new();
        Logger.Info(this, $"Interpreter `{Identity}` in mode `{Mode}` is starting to interpret.");
        
        for (; Position < Statements.Count; ++Position)
        {
            var node = Statements[Position];

            if (node is null)
                throw new InterpreterException("internal: parser problem... null node?");

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            _ = node.Take(this);
#if DEBUG
            sw.Stop();
#endif

#if DEBUG
            if (!productionTimes.ContainsKey(node.GetType()))
                productionTimes[node.GetType()] = sw.ElapsedMilliseconds;
            else
            {
                productionTimes[node.GetType()] += sw.ElapsedMilliseconds;
            }
#endif
        }

#if DEBUG
        foreach (var time in productionTimes)
        {
            Console.WriteLine($"{time.Key.Name} -- {time.Value}ms ({time.Value / 1000}s)");
        }
#endif

        return Undefined;
    }

    public object VisitAssignment(Assignment assignment)
    {
        var name = assignment.Decl.Name;
        var value = assignment.Statement.Take(this);

        var isConst = (assignment.Decl as Variable)?.IsConstant ?? assignment.IsConst;

        var instance = new DeeObject<object>(value)
        {
            IsConst = isConst
        };

        if (value is Declaration decl)
        {
            if (assignment.Decl.Type != decl.Type)
            {
                Panic($"cannot assign an instance of '{decl.Type}' to '{assignment.Decl.Type}'");
            }
            // OK!
        }

        if (value is IStruct @struct)
        {
            if (isConst)
            {
                @struct.ConstInstance = true;
            }

            if (assignment.Decl.Type.Name != @struct.Name)
            {
                Panic($"cannot assign an instance of '{@struct.Name}' to '{assignment.Decl.Type}'");
            }
            // OK!
        }

        if (value is ReturnValue @return)
            value = @return.Value;

        if (ActiveScope != null && _global.Contains(name))
        {
            if (assignment.Decl is not Variable var)
                throw new NotImplementedException("Handle the assignment not being a variable.");

            if (var.IsInitialization)
            {
                // cannot assign to a variable with the same name as one in an outer scope.
                Panic($"The variable `{name}` already exists in the scope `{_global.Name}`.");
            }

            return _global.Assign(this, name, instance);
        }

        return ActiveScope != null
            ? ActiveScope.Assign(this, name, instance)
            : _global.Assign(this, name, instance);
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
        if (CurrentScope.Contains(call.Identifier) || GlobalScope().Contains(call.Identifier))
        {
            var value = GetFromEither(call.Identifier);

            if (value is UserDefinedFunction userFunction)
            {
                Logger.Info(this, $"Calling user defined function `{userFunction.Name}`");
                // do shit
                return userFunction.Execute(this, call.Arguments.ToList());
            }

            if (value is IStruct structDecl)
            {
                Logger.Info(this, $"Calling object constructor `{structDecl.Name}`");
                var scopeId = structDecl.Name;
                var ctor = structDecl.GetValue("construct") as IStructFunction;

                if (ctor is null || structDecl.GetValue("construct") is Undefined)
                {
                    Logger.Info(this, $"Object `{structDecl.Name}` has no constructor."
                     + " Constructing it using default");
                    var result = new UserDefinedStruct(scopeId, false);
                    result.Populate(this, structDecl);
                    return result;
                }

                var @struct = new UserDefinedStruct(scopeId, false);
                @struct.Populate(this, structDecl);
                _ = ctor.Execute(this, @struct, call.Arguments.ToList());
                return @struct;
            }

            Panic($"The variable `{call.Identifier}` is not callable. (it is of type `{value.GetType().Name}`)");
        }

        if (!_calls.TryGetDefinition(call.Identifier, out ICallable function))
            return Undefined;
        // An arity of -1 means the function expects no specific number of arguments.
        if (function.Arity != -1 && call.Arguments.Length != function.Arity)
        {
            Panic($"'{call.Identifier}' expects {function.Arity} arguments, but {call.Arguments.Length} were supplied.");
            return Undefined;
        }
        var arguments = new List<Literal>();
        foreach (var arg in call.Arguments)
        {
            if (arg is FunctionCall paramCall)
            {
                var result = paramCall.Take(this);
                // FIXME: not all functions may return a string...
                var literal = new Literal(DToken.MakeVar(TokenType.String), TypeHint.String, result);
                arguments.Add(literal);
                continue;
            }

            if (arg is Literal paramLiteral)
            {
                arguments.Add(paramLiteral);
                continue;
            }

            if (arg is Variable variable)
            {
                // builtin functions dont have their own scope.
                var @var = GetFromEither(variable.Name);
                arguments.Add(new Literal(DToken.MakeVar(TokenType.String), TypeHint.String, @var));
                continue;
            }

            if (arg is VariableAccess access)
            {
                arguments.Add(new Literal(DToken.MakeVar(TokenType.String), TypeHint.String, access.Take(this)));
                continue;
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

    private void DumpScopes()
    {
        if (AllowsStdout)
        {
            var g = Newtonsoft.Json.JsonConvert.SerializeObject(_global);
            var l = Newtonsoft.Json.JsonConvert.SerializeObject(ActiveScope);

            Console.WriteLine($"Globals: {g}\nLocals: {l}");
        }
    }

    public object VisitVariable(Variable variable)
    {
        var local = ActiveScope;

        if (local is not null && local.Contains(variable.Name))
        {
            return local.GetValue(variable.Name);
        }

        if (_global.Contains(variable.Name))
        {
            return _global.GetValue(variable.Name);
        }

        Panic($"The variable `{variable.Name}` does not exist in any scope.");
        return null!;
    }

    public object VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var name = functionDeclaration.Identifier;
        var args = functionDeclaration.Arguments;
        var body = functionDeclaration.Body;

        var udf = new UserDefinedFunction(name, body, args);
        CurrentScope.Assign(this, name, new DeeObject<object>(udf) { IsConst = true });
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
            Panic($"cannot perform addition between `{left}` and `{right}`");
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
            Panic($"cannot perform subtraction between `{left}` and `{right}`");
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
            Panic($"cannot perform multiplication between `{left}` and `{right}`");
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
            Panic($"cannot perform division between `{left}` and `{right}`");
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
        var declaration = new UserDefinedStruct(structDeclaration.Identifier, true);

        foreach (var decl in structDeclaration.Declarations)
        {
            if (decl is FunctionDeclaration func)
            {
                var smf = new StructMemberFunction(func.Identifier,
                    func.Body,
                    func.Arguments);
                Logger.Info(this, $"Defined function `{func.Name}` in struct `{declaration.Name}`");
                declaration.Define(this, smf.Name, new DeeObject<object>(smf));
            }

            if (decl is PropertyDeclaration prop)
            {
                Logger.Info(this, $"Defined {(prop.IsStatic ? "static" : "instance")} property `{prop.Name}` in struct `{declaration.Name}`");
                declaration.Define(this, prop.Name, 
                    new DeeObject<object>(new UserDefinedProperty(this, prop)));
            }
        }

        CurrentScope.Assign(this, declaration.Name, new DeeObject<object>(declaration) { IsConst = true });
        return declaration;
    }

    public object VisitVariableAccess(VariableAccess variableAccess, out IScope? scope, bool isAssignment)
    {
        IScope? current = null;
        object? result = null;

        for (var i = 0; i <= variableAccess.Tree.Count; i++)
        {
            if (current is not null && current.ConstInstance)
            {
                if (isAssignment)
                    Panic(variableAccess.Tree.First(), "cannot assign to constant instance");
            }

            var lastIteration = (i + 1 >= variableAccess.Tree.Count);
            var firstIteration = (i == 0);

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
                        Panic($"undefined member `{call.Identifier}` on instance `{current}`");
                        scope = null!;
                        return null!;
                    }

                    var args = call.Arguments.ToList();
                    val = smf.Execute(this, (IStruct)current, args);
                }

                if (val is not IScope currentScope)
                {
                    if (lastIteration)
                    {
                        result = val;
                        break;
                    }
                    Panic($"The function `{call.Identifier}` returns a value that is not accessible. (cannot `::` an instance of `{val.GetType()}`)");
                    scope = null;
                    return null!;
                }
                current = currentScope;
                continue;
            }

            if (current is null)
            {
                var it = variableAccess.Tree[i].Take(this);
                if (it is IScope nextScope)
                    current = nextScope;
            }
            else
            {
                if (variableAccess.Tree[i] is not Variable variable)
                    throw new InterpreterException("cannot access a non-scoped type.");
                result = current.GetValue(variable.Name);

                if (result is IScope s)
                {
                    current = s;
                    continue;
                }
                if (result is UserDefinedProperty prop)
                {
                    var inner = prop.Get();
                    if (inner is IScope innerScope)
                    {
                        current = innerScope;
                        continue;
                    }

                    scope = current;
                    return inner;
                }
                break;
            }
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

    public object VisitVariableAccessAssignment(VariableAccessAssignment variableAccessAssignment)
    {
        _ = VisitVariableAccess(variableAccessAssignment.Access, out IScope? scope, true);

        if (scope is null)
            throw new NotImplementedException("Variable accesses scope is null...");

        var variableName = variableAccessAssignment.Access.Tree.Last();

        if (variableName is not Variable var)
        {
            // The value being assigned to is not a variable.
            Panic($"Cannot assign to {variableName}");
            return null!;
        }

        scope.Assign(this, var.Name, new DeeObject<object>(Visit(variableAccessAssignment.Operand)), var);

        return scope.GetValue(variableName);
    }

    public object Visit(Statement st)
        => st.Take(this);

    public object VisitEntityIndex(EntityIndex entityIndex)
    {
        var entity = Visit(entityIndex.Entity);

        if (entity is not IDictionary<object, object> or IList<object>)
        {
            Panic($"cannot index an entity of type {entity.GetType()}");
        }

        var current = entityIndex
            .Indices
            .Select(Visit)
            .Aggregate<object?, object?>(null, (current1, index) =>
            {
                if (index != null) return IndexEntity(current1 ?? entity, index);
                Panic("internal problem: index was null");
                return null!;
            });

        return current ?? Undefined;
    }

    private Dictionary<string, string>? _knownModules;

    private Dictionary<string, string> InitializeModuleLookupTableIfNeeded()
    {
        if (_knownModules is not null) return _knownModules;
        
        // check if all available files have been loaded yet or not.
        if (Configuration.GetOption("module-paths") is not { } paths)
        {
            Panic("No module paths are loaded! This means imports cannot be used.\n" + 
                  "Use Configuration.RegisterDefaultOptions with the option `module-paths` to add some.");
            return new Dictionary<string, string>();
        }

        _knownModules = new Dictionary<string, string>();
        var cwd = Directory.GetCurrentDirectory();    
        
        foreach (var path in paths)
        {
            var directory = path;
            
            if (!Path.IsPathRooted(path))
            {
                directory = $"{cwd}/{path}";
            }
            
            if (!Directory.Exists(directory))
            {
                ModLog($"path within module-paths does not exist. [{directory}]");
                continue;
            }

            var files = Directory.GetFiles(directory, "*.dl", SearchOption.AllDirectories).ToList();
            // remove .dl from the files

            files.ForEach(x =>
            {
                var info = new FileInfo(x);
                _knownModules.Add(info.Name.Replace(".dl", ""), info.FullName);
            });
        }

        return _knownModules;
    }
    
    public object VisitModuleImport(ModuleImport import)
    {
        _knownModules ??= InitializeModuleLookupTableIfNeeded();

        if (!_knownModules.ContainsKey(import.FileName))
        {
            Panic(import, $"No such file `{import.FileName}` in any lookup path.");
            return Undefined;
        }
        
        // initialize a new interpreter to parse the file.
        var moduleInterpreter = new Interpreter(_knownModules[import.FileName]) { PreventExit = PreventExit };
        
        // If this code is executing, it means that lexing, parsing and interpreting didn't go wrong.
        // Lets check if it's a wildcard.

        if (import.Members.Contains("*"))
        {
            Logger.Info(this, $"Importing everything from the module `{import.FileName}`");
            // Simply join it's global scope with ours.
            _global.Combine(moduleInterpreter.GlobalScope());
            return Undefined;
        }
        
        // We need to siv through it's scope ourself to find each requested thing.
        foreach (var expected in import.Members)
        {
            var value = moduleInterpreter.GlobalScope().GetValue(expected);
            if (value is null or Undefined)
            {
                Panic($"The module `{import.FileName}` contains no definition for `{expected}`");
                return null!;
            }

            _global.Assign(this, expected, new DeeObject<object>(value) { IsConst = true });
        }

        return Undefined;
    }

    public object VisitModuleAssignment(AssignedModuleImport import)
    {
        _knownModules ??= InitializeModuleLookupTableIfNeeded();

        if (!_knownModules.ContainsKey(import.Name))
        {
            Panic($"No such file `{import.Name}` in any lookup path.");
            return Undefined;
        }
        
        // initialize a new interpreter to parse the file.
        var moduleInterpreter = new Interpreter(_knownModules[import.Name]) { PreventExit = PreventExit };

        // Combine the interpreted scope into the new module instance.
        var module = new RuntimeStorage($"<module '{import.Name}'>");
        module.Combine(moduleInterpreter.Globals());

        // TODO: this doesn't work... If you use an inline import statement 
        // the identifier is for SOME reason not assigned into any scope.
        CurrentScope.Assign(this, import.Assignee.Name, new DeeObject<object>(module) { IsConst = true });
        
        return module;
    }

    public object VisitPropertyDeclaration(PropertyDeclaration declaration)
    {
        var property = new UserDefinedProperty(this, declaration);
        // TODO: propertys should have the `const` modifier too.
        CurrentScope.Assign(this, declaration.Name, new DeeObject<object>(property));
        return property;
    }

    public object IndexEntity(object entity, object accessor)
    {
        if (entity is List<object> list)
        {
            if (accessor is not long l)
            {
                Panic($"cannot access type `list` with `{accessor.GetType()}`");
                return null!;
            }

            return list[(int)l];
        }

        if (entity is Dictionary<object, object> dict)
        {
            return dict[accessor];
        }

        Panic("Type is not able to be indexed");
        return null!;
    }

    public void Panic(Statement source, string message)
    {
        _errors.CreateWithMessage(source, message, true);
        _errors.DisplayErrors();

        if (!PreventExit)
        {
            Environment.Exit(-1);
        }
    }
}