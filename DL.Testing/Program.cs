using DL.Lexer;

string script = @"
'windows' = [
    'garbage',
    'terrible'
]

'dict' = {
    'attributes': [ 'nice' ],
    'size': 2
}
";

var lexer = new DLexer(script);

var tokens = lexer.Lex();

foreach (var token in tokens)
{
    if (token.Type == TokenType.Invalid)
    {
        Console.WriteLine($"invalid token: {token.Literal}");
        continue;
    }

    Console.WriteLine(token);
}
