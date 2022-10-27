using Runtime;
using Runtime.Interpreting;
using Runtime.Interpreting.Api;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;
using System.Diagnostics;

// REPL version.

if (args.Length >= 1)
{
    var file = args[0];
    if (!File.Exists(file))
    {
        Console.WriteLine($"file could not be found `{file}`");
    }
    else
    {
        var contents = File.ReadAllText(file);
        var cfg = DlRuntime.ProcessConfig(contents).Config;

        foreach (var (key, value) in cfg.Elements)
        {
            Console.WriteLine($"{ProcessDValue(key)}: {ProcessDValue(value)}");
        }
    }

    return;
}

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
        Stopwatch sw = Stopwatch.StartNew();
        tokens = new DLexer(input).Lex();
        ast = new DParser(tokens).Parse();
        config = new Interpreter().Interpret(ast);
        Console.WriteLine($"took {sw.ElapsedMilliseconds}ms to process contents!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"an exception occured: {ex.Message}");
        continue;
    }

    foreach (var (key, value) in config.Elements)
    {
        Console.WriteLine($"{ProcessDValue(key)}: {ProcessDValue(value)}");
    }
}

string ProcessDValue(DValue value)
{
    if (value.Instance is List<DValue> list)
    {
        return $"[{string.Join(", ", list)}]";
    }

    if (value.Instance is Dictionary<DValue, DValue> dict)
    {
        string result = "{";

        var keys = dict.Keys.Select(x => x.ToString()).ToList();
        var values = dict.Values.Select(x => ProcessDValue(x)).ToList();

        for (int i = 0; i < keys.Count; ++i)
        {
            if ((i + 1) == keys.Count)
            {
                result += $"{keys[i]}: {values[i]}";
            }

            result += $"{keys[i]}: {values[i]}, ";
        }

        return result + '}';
    }

    return value.ToString();
}