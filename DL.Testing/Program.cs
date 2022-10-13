using DL;

string script = @"
'windows' = [
    'terrible',
    'garbage'
];

'version' = 1.02;

'dict' = {
    'nested': {
        'value': 102
    },
    'version': 1.02
};
";

var context = DLRuntime.ProcessConfig(script);

if (context.Errors.Count >= 1)
{
    Console.WriteLine($"{context.Errors.Count} error(s)");
    context.Errors.ForEach(x => Console.WriteLine(x.Message));
}

var config = context.Config;

Console.WriteLine(config.Count);




