using Runtime.Parser.Errors;
using Runtime.Interpreting.Api;
using Runtime.Interpreting.Calls;
using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting;

public class Interpreter : ISyntaxTreeVisitor<DValue>
{
    private readonly IConfig _config;
    private readonly CallCenter _calls;

    public Interpreter()
    {
        _config = new DLConfig();
        _calls = new CallCenter();
    }

    public IConfig Interpret(List<DNode> nodes)
    {
        // every node should be an assignment.

        foreach (var node in nodes)
        {
            DValue result = Eval(node);
            if (result.Instance is not KeyValuePair<DValue, DValue> kvp)
            {
                throw new InterpreterException($"expected key-value pair, got {result.Instance?.GetType()}");
            }

            ((DLConfig)_config).AddElement(kvp);
        }

        return _config;
    }

    public DValue VisitAssignment(Assignment assignment)
    {
        DValue? key = assignment.Key.Take(this);
        DValue? value = assignment.Value.Take(this);

        return new DValue(new KeyValuePair<DValue, DValue>(key, value));
    }

    public DValue VisitDict(Dict dict)
    {
        var dictValues = new Dictionary<DValue, DValue>();

        foreach (var element in dict.Members)
        {
            var (k, v) = (Accept(element.Key), Accept(element.Value));
            dictValues.Add(k, v);
        }

        return dictValues;
    }

    public DValue VisitDictAssignment(DictAssignment assignment)
    {
        throw new NotImplementedException();
    }

    public DValue VisitFunctionCall(FunctionCall call)
    {
        if (!_calls.TryGetDefinition(call.Identifier, out var function))
        {
            throw new BadIdentifierException($"there is no function defined with name `{call.Identifier}`");
        }

        return function.Execute(this, call.Arguments).Take(this);
    }

    public DValue VisitList(List list)
    {
        var dValues = new List<DValue>();

        foreach (var element in list.Literals)
        {
            var v = Accept(element);
            dValues.Add(v);
        }

        return dValues;
    }

    public DValue VisitLiteral(Literal literal)
    {
        return (literal.Object, literal.Sentiment.Type) switch
        {
            (_, TokenType.Boolean) => (bool)literal.Object,
            (_, TokenType.String) => (string)literal.Object,
            (_, TokenType.Decimal) => (decimal)literal.Object,
            (_, TokenType.Number) => (long)literal.Object,
            _ => false
        };
    }

    internal DValue Accept(DNode node)
        => node.Take(this);

    internal DValue Eval<T>(T node) where T : DNode
        => node.Take(this);
}