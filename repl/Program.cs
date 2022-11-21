﻿using Runtime.Interpreting;
using Runtime.Lexer;
using Runtime.Parser;

// REPL version.

Console.WriteLine("DL REPL -- v1.0.0\n");
var interpreter = new Interpreter();

while (true)
{
    var input = Console.ReadLine();

    var lexer = new DLexer(input!);
    var tokens = lexer.Lex();
    var parser = new DParser(tokens, input!.Split('\n').ToList());
    var ast = parser.Parse();

    var evalResult = interpreter.Interpret(ast);
    Console.WriteLine($"Result: {evalResult}");

    var errs = parser.Errors;

    if (errs.Errors.Count >= 1)
    {
        Console.WriteLine($"{errs.Errors.Count} error(s) occured.");
        foreach (var err in errs.Errors) Console.WriteLine(err.Message);
    }
}