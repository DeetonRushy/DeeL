namespace DL.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void LexStringWorks()
        {
            var s = "'window' = 'window';'size=size'='size'";
            var lexer = new DLexer(s);

            var tokens = lexer.Lex();

            Assert.AreEqual("window", tokens[0].Lexeme);
            Assert.AreEqual(tokens[0].Lexeme, tokens[2].Lexeme);

            Assert.AreEqual(TokenType.LineBreak, tokens[3].Type);

            Assert.AreEqual("size=size", tokens[4].Lexeme);
            Assert.AreEqual("size", tokens[6].Lexeme);
        }
    }
}