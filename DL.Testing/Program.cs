using DL.Lexer;
using DL.Parser;
using DL.Parser.Production;

string script = @"
'windows' = [
    WINVER,
    true,
    __cpp_version
]

'version' = 1.02

'dict' = {
    'nested': {
        'value': 102
    },
    'version': 1.02
}
";

var lexer = new DLexer(script);

var tokens = lexer.Lex();

tokens.ForEach(x =>
{
    if (x.Type != TokenType.Eof && x.Type != TokenType.Invalid)
        Console.WriteLine(x);
});

var parser = new DParser(tokens);

List<DNode> ast = parser.Parse();

parser._error.DisplayErrors();

foreach (var node in ast)
{
    Console.WriteLine(node.Debug());
}




