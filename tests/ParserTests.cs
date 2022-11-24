

using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Tests;

[TestClass]
public class ParserTests
{
    private List<Statement> GetAst(string src)
    {
        var lexer = new DLexer(src);
        var tokens = lexer.Lex();

        var parser = new DParser(tokens);
        return parser.Parse();
    }

    [TestMethod]
    public void TestLetWithString()
    {
        string src = "let src: string = 'src';";
        var ast = GetAst(src);

        Assert.AreNotEqual(0, ast.Count);
        Assert.AreEqual(ast[0].GetType(), typeof(Assignment));

        var ass = ast[0] as Assignment;
        Assert.IsNotNull(ass);

        Assert.AreEqual("src", ass.Decl.Name);
        Assert.AreEqual(ass.Statement.GetType(), typeof(Literal));

        var literal = ass.Statement as Literal;
        Assert.IsNotNull(literal);

        Assert.IsTrue(literal.Sentiment.Lexeme == "src");
    }

    [TestMethod]
    public void TestLetWithAnnotations()
    {
        var src = "let src: string = 'hello';";
        var ast = GetAst(src);

        // Assignment.Variable.TypeHint should be 'string'

        var ass = ast[0] as Assignment;
        Assert.IsNotNull(ass);

        Assert.AreEqual(ass.Decl.Type.Name, TypeHint.String.Name);
        Assert.AreEqual(ass.Decl.Name, "src");

        var literal = ass.Statement as Literal;
        Assert.IsNotNull(literal);

        Assert.AreEqual("hello", literal.Object);
    }
}
