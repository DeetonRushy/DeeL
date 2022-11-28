using Runtime.Interpreting;
using Runtime.Parser.Production;
using Runtime.Parser.Production.Conditions;
using Runtime.Parser.Production.Math;

namespace Runtime.Parser;

/* design choice taken from SharpLox (https://github.com/Jay-Madden/SharpLox) */

/// <summary>
/// An interface that, when implemented, represents an interpreter.
/// </summary>
/// <typeparam name="T">The type each visit should return.</typeparam>
public interface ISyntaxTreeVisitor<out T>
{
    public T VisitAssignment(Assignment assignment);
    public T VisitLiteral(Literal literal);
    public T VisitList(List list);
    public T VisitDictAssignment(DictAssignment assignment);
    public T VisitDict(Dict dict);
    public T VisitFunctionCall(FunctionCall call);
    public T VisitModuleIdentity(ModuleIdentity moduleIdentity);
    public T VisitVariable(Variable variable);
    public T VisitFunctionDeclaration(FunctionDeclaration functionDeclaration);
    public T VisitBlock(Block block);
    public T VisitReturnStatement(ReturnValue returnValue);
    public T VisitBreakPoint(ExplicitBreakpoint bp);
    public T VisitGrouping(Grouping grouping);
    public T VisitAddition(Addition addition);
    public T VisitSubtraction(Subtraction subtraction);
    public T VisitMultiplication(Multiplication multiplication);
    public T VisitDivision(Division division);
    public T VisitStructDeclaration(StructDeclaration structDeclaration);
    public T VisitVariableAccess(VariableAccess variableAccess, out IScope? scope);
    public T VisitIfStatement(IfStatement conditional);
    public T VisitIsEqualsComparison(IsEqual isEqual);
    public T VisitIsNotEquals(IsNotEqual isNotEqual);
    public T VisitWhileLoop(WhileStatement whileStatement);
    public T VisitVariableAccessAssignment(VariableAccessAssignment variableAccessAssignment);
    public T VisitEntityIndex(EntityIndex entityIndex);
    public T VisitModuleImport(ModuleImport import);
}
