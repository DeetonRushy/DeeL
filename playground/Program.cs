using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Interpreting;
using Runtime;
using System.Diagnostics;

DContext ctx = null!;

try
{
    var sw = Stopwatch.StartNew();

    var source = @"

fn getName() -> string {
  return 'name';
}

writeln(getName());
";

    ctx = DlRuntime.Run(source);

    sw.Stop();
    Console.WriteLine($"Total execution time: {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

ctx.ErrorHandler.DisplayErrors();

Console.WriteLine("**** GLOBALS ****\n");
foreach (var g in ctx.Interpreter.Globals())
{
    Console.WriteLine($"{{ {g.Key}: {g.Value} }}");
}