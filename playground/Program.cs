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
mod 'dl';

fn getVersionAndPrintName(name: string) -> string {
  writeln(name);
  return '1.2.32';
}

let v = getVersionAndPrintName('deeton');
writeln(v);

let not$callable = 1;
not$callable();
";

    ctx = DlRuntime.Run(source);

    sw.Stop();
    Console.WriteLine($"Total execution time: {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
}
catch (Exception ex)
{
    Console.WriteLine("exception: " + ex.Message);
}

ctx.ErrorHandler.DisplayErrors();

Console.WriteLine("**** GLOBALS ****\n");
foreach (var g in ctx.Interpreter.Globals())
{
    Console.WriteLine($"{{ {g.Key}: {g.Value} }}");
}