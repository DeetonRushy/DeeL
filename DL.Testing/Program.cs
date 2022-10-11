using DL.Lexer;

string script = @"
# comments
";

DLexer lexer = new DLexer(script);

var tokens = lexer.Lex();

tokens.ForEach(x => Console.WriteLine($"{x}: {x.Lexeme.Contents()}"));
