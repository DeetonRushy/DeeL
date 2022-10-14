using DL;
using System.Diagnostics;

string script = @"
'directory' = $CurrentWorkingDirectory;
'size' = 1029;
";

Stopwatch sw = Stopwatch.StartNew();
var context = DLRuntime.ProcessConfig(script);
sw.Stop();

Console.WriteLine($"took {sw.ElapsedMilliseconds}ms to process the config file!");

if (context.Errors.Count >= 1)
{
    Console.WriteLine($"{context.Errors.Count} error(s)");
    context.Errors.ForEach(x => Console.WriteLine(x.Message));
}

var config = context.Config;

Console.WriteLine(config.Count);




