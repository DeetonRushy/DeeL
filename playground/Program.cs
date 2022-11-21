using Runtime;
using System.Diagnostics;

DContext ctx = null!;

try
{
    var sw = Stopwatch.StartNew();

    var source = @"
let apple: integer = cwd;
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