using DL.Lexer;
using DL.Parser;
using DL.Parser.Production;

string script = @"
'windows' = 'garbage'
";

var lexer = new DLexer(script);

var tokens = lexer.Lex();

var parser = new DParser(tokens);

List<DNode> ast = new List<DNode>();

try
{
    ast = parser.Parse();
}
catch 
{
    Console.WriteLine($"failure. {parser._error.Errors.Count} error(s)");

    if (parser._error.Errors.Count >= 1)
        parser._error.DisplayErrors();
}




