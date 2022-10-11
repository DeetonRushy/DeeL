using DL.Lexer;

string script = @"'height' = 12";

var lexer = new DLexer(script);

var tokens = lexer.Lex();

foreach (var token in tokens)
{
    Console.Write(token);
    var contents = token.Lexeme.Contents();
    Console
        .Write($" | Content: {contents} (LC: {contents.Length}, Real: {token.Lexeme.Difference()})\n");
}
