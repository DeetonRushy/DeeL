using DL.Lexer;

string script = @"102 = 12";

DLexer lexer = new DLexer(script);

var tokens = lexer.Lex();

foreach (var token in tokens)
{
    Console.Write(token);
    var contents = token.Lexeme.Contents();
    Console
        .Write($" | Content: {contents} (LC: {contents.Length}, Real: {token.Lexeme.Difference()})\n");
}
