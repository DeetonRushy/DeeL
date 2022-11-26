namespace Runtime.Tests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void LexStringWorks()
        {
            var s = "let content = 'hello, world!'";
            var tokens = DlRuntime.ViewTokens(s);

            Assert.IsNotNull(tokens);
            Assert.AreEqual(TokenType.Let, tokens[0].Type);
            Assert.AreEqual("content", tokens[1].Lexeme);
            Assert.AreEqual(TokenType.Equals, tokens[2].Type);

            var literal = tokens[3];

            Assert.AreEqual(TokenType.String, literal.Type);
            Assert.AreEqual("hello, world!", literal.Lexeme);
        }

        [TestMethod]
        public void LexDecimalWorks()
        {
            const decimal large = 10.938249428242m;
            var s = $"let dec = {large};";
            var lexer = new DLexer(s);

            var tokens = lexer.Lex();

            Assert.AreEqual(large, tokens[3].Literal);
        }

        [TestMethod]
        public void LexNumberWorks()
        {
            const long large = long.MaxValue - 1;
            var s = $"let long = {large}";

            var tokens = new DLexer(s).Lex();

            Assert.AreEqual(large, tokens[3].Literal);
        }

        [TestMethod]
        public void LexBooleanWorks()
        {
            var s = "let condition = false;";
            var tokens = new DLexer(s).Lex();

            Assert.AreEqual("false", tokens[3].Lexeme);
        }
    }
}