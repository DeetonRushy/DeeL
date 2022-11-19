using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Interpreting;
using Runtime;
using System.Diagnostics;
using playground;

var sw = Stopwatch.StartNew();
Interpreter interpreter = new();

var source = @"
fn myFunction(arg) {
    writeln(arg);
}

myFunction('Hello, World!');
";

var tokens = new DLexer(source).Lex();
var parser = new DParser(tokens);
var ast = parser.Parse();

var instance = interpreter.Interpret(ast);
sw.Stop();

Console.WriteLine(instance);

parser.Errors.DisplayErrors();
Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to execute!");

var syntaxTokens = new DLexer(source) { MaintainWhitespaceTokens = true }.Lex();
var pv = new PrettyView(syntaxTokens);
var pretty = pv.Output();

Console.WriteLine($"\nPretty View:\n\n {pretty}");