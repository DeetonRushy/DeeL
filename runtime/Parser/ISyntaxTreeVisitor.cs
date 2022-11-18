using Runtime.Parser.Production;

namespace Runtime.Parser;

/* design choice taken from SharpLox (https://github.com/Jay-Madden/SharpLox) */

/// <summary>
/// An interface that, when implemented, represents an interpreter.
/// </summary>
/// <typeparam name="T">The type each visit should return.</typeparam>
public interface ISyntaxTreeVisitor<T>
{
    public T VisitAssignment(Assignment assignment);
    public T VisitLiteral(Literal literal);
    public T VisitList(List list);
    public T VisitDictAssignment(DictAssignment assignment);
    public T VisitDict(Dict dict);
    public T VisitFunctionCall(FunctionCall call);
    public T VisitSingleEvaluation(SingleEval evaluation);
    public T VisitModuleIdentity(ModuleIdentity moduleIdentity);
}
