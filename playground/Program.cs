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
mod 'titties';

fn doPrint(text) {
  writeln(text);
  return text;
}

let res = doPrint('hello, world');
doPrint(res);
let d = {'hello': 2};
let l = [1, 2, 3, 4.92842];
";

    ctx = DlRuntime.Run(source);
    ctx.ErrorHandler.DisplayErrors();

    sw.Stop();
    Console.WriteLine($"Total execution time: {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Console.WriteLine("**** GLOBALS ****\n");
foreach (var g in ctx.Interpreter.Globals())
{
    Console.WriteLine($"{{ {g.Key}: {g.Value} }}");
}