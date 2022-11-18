using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Interpreting;
using Runtime;
using System.Diagnostics;

var sw = Stopwatch.StartNew();
Interpreter interpreter = new();

var source = @"
mod 'json@0.0.1';

'version-info' = {
  'initial': '0.0.1',
  'versions': [
    '0.0.1-dev'
  ]
};
'main-version' = access('version-info', 'versions', 0);
'initial-version' = access('version-info', 'initial');
";

var tokens = new DLexer(source).Lex();
var parser = new DParser(tokens);
var ast = parser.Parse();

var instance = interpreter.Interpret(ast);
sw.Stop();

Console.WriteLine(instance);

parser.Errors.DisplayErrors();
Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to execute!");