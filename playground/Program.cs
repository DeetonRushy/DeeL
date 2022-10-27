using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Interpreting.Meta;

Interpreter<TestClass> interpreter = new();

var source = @"'Name' = 'Deeton';'Age' = 19;'Tags' = ['Human', 'Brown Eyes'];";
var tokens = new DLexer(source).Lex();
var ast = new DParser(tokens).Parse();

TestClass instance = interpreter.Interpret(ast);

Console.WriteLine($"{instance.Name} is {instance.Age}");


class TestClass
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<string> Tags { get; set; }
}