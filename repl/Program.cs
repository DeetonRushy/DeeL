using Runtime.Interpreting;
using Runtime.Lexer;
using Runtime.Parser;

// REPL version.

Console.WriteLine("DL REPL -- v1.0.0\n");

while (true)
{
    var input = Console.ReadLine();

    var lexer = new DLexer(input!);
    var tokens = lexer.Lex();
    var parser = new DParser(tokens);
    var ast = parser.Parse();
    var interpreter = new Interpreter(ast);

    var evalResult = interpreter.Interpret();
    Console.WriteLine($"Result: {evalResult}");

    var errs = parser.Errors;

    if (errs.Errors.Count >= 1)
    {
        Console.WriteLine($"{errs.Errors.Count} error(s) occured.");
        foreach (var err in errs.Errors) Console.WriteLine(err.Message);
    }
}