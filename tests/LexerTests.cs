using Runtime.Lexer;

namespace Runtime.Tests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void LexStringWorks()
        {
            var s = "'window' = 'window';'size=size'='size';";
            var lexer = new DLexer(s);

            var tokens = lexer.Lex();

            Assert.AreEqual("window", tokens[0].Lexeme);
            Assert.AreEqual(tokens[0].Lexeme, tokens[2].Lexeme);

            Assert.AreEqual(TokenType.LineBreak, tokens[3].Type);

            Assert.AreEqual("size=size", tokens[4].Lexeme);
            Assert.AreEqual("size", tokens[6].Lexeme);
        }

        [TestMethod]
        public void LexDecimalWorks()
        {
            const decimal large = 10.938249428242m;
            var s = $"10.0 = 'number';'number'={large};";
            var lexer = new DLexer(s);

            var tokens = lexer.Lex();

            Assert.AreEqual(10.0m, tokens[0].Literal);
            Assert.AreEqual("number", tokens[2].Lexeme);

            Assert.AreEqual(large, tokens[6].Literal);
        }

        [TestMethod]
        public void LexNumberWorks()
        {
            const long large = long.MaxValue - 1;
            var s = $"'long' = 923842;'large-long' = {large}";

            var tokens = new DLexer(s).Lex();

            Assert.AreEqual(923842L, tokens[2].Literal);
            Assert.AreEqual(large, tokens[6].Literal);
        }

        [TestMethod]
        public void LexBooleanWorks()
        {
            var s = "'true'=true;'false'=false;";
            var tokens = new DLexer(s).Lex();

            Assert.AreEqual(TokenType.Boolean, tokens[2].Type);
            Assert.AreEqual("true", tokens[2].Lexeme);

            Assert.AreEqual(TokenType.Boolean, tokens[6].Type);
            Assert.AreEqual("false", tokens[6].Lexeme);
        }
    }
}