using Runtime;
using System.Diagnostics;

DContext ctx;

try
{
    var sw = Stopwatch.StartNew();
    ctx = DlRuntime.Run(File.ReadAllText($"C:\\Users\\{Environment.UserName}\\source\\repos\\DeeL\\runtime\\markup1.dl"), false);

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

foreach (var (key, value) in ctx.Interpreter.Globals())
{
    Console.WriteLine($"{{ {key}: {value} }}");
}