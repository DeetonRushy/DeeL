using DL;
using DL.Interpreting;
using DL.Lexer;
using DL.Parser;
using DL.Parser.Production;
using System.Diagnostics;

// REPL version.

Console.WriteLine("Dl REPL");

while (true)
{
    Console.Write(">> ");
    var input = Console.ReadLine() ?? string.Empty;
    Console.WriteLine();

    if (input == "_quit")
        break;

    List<DToken> tokens;
    List<DNode> ast;
    IConfig config;

    try
    {
        tokens = new DLexer(input).Lex();
        ast = new DParser(tokens).Parse();
        config = new Interpreter().Interpret(ast);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"an exception occured: {ex.Message}");
        continue;
    }

    foreach (var (key, value) in config.Elements)
    {
        Console.WriteLine($"{key}: {value}");
    }
}




