using Runtime;
using System.Diagnostics;

DContext ctx;

try
{
    var sw = Stopwatch.StartNew();
    var goal = @"
from 'std.io' import { File, Result, Okay, Failure };

fn openFile(name: string) -> Result<File> {
  let result: Result<> = File::open(name);

  if (result.ok()) {
    return Okay(result::value());
  }

  return Failure(result::error());
}

from 'std' import { Env };
from 'std.ext' import { ArgParser, Namespace, quit };

let argv: list<string> = Env::argv();
let parser: ArgParser = ArgParser::new(argv);

parser::add_required('n', 'name', {'desc'='the file to read the contents of'});
parser::add_optional('v', 'verbose', {'desc'='enable verbose mode'});

let results: Namespace = parser::compute({'capture-errors': true});
if (results::errors::size() != 0) {
  writeln(@'errors: {results::errors}');
  quit(-1);
}

let path: string = results::name;
let handle = openFile(path);

if (!path::ok()) {
  writeln(@'failed to discover file: {handle::error()::message}');
  quit(-2);
}

for (let line in iterate(handle::value()::read_lines())) {
  writeln(@'{line}\n');
}

if (results::verbose) {
  writeln(@'**** iterated {handle::value()::count} lines ****');
}

";

    const string new_source = @"
from 'std' import {*};
";
    ctx = DlRuntime.Run(new_source, false);

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