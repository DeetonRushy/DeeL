using Runtime;
using System.Diagnostics;

DContext ctx;

try
{
    var sw = Stopwatch.StartNew();

    const string newSource = @"
from 'std' import {*};

let arr: list = [1, 2, 3, 4];
writeln(arr[1]);
";
    ctx = DlRuntime.Run(newSource, false);

    sw.Stop();
    Console.WriteLine($"\n\nTotal execution time: {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
}
catch (Exception ex)
{
    Console.WriteLine("exception: " + ex.Message);
    throw;
}

ctx.ErrorHandler.DisplayErrors();

Console.WriteLine("**** GLOBALS ****\n");
foreach (var g in ctx.Interpreter.Globals())
{
    Console.WriteLine($"{{ {g.Key}: {g.Value} }}");
}