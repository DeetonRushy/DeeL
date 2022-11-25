using Runtime;
using System.Diagnostics;

DContext ctx = null!;

try
{
    var sw = Stopwatch.StartNew();

    var source = @"
mod 'DL';

object User {
  fn construct(self: User) -> void {
    self::name = 'user';
  }
}

let user: User = User('hello');
writeln(user::name);
";
    ctx = DlRuntime.Run(source, false);

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