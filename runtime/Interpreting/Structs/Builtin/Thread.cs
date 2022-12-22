using Runtime.Parser.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Interpreting.Structs.Builtin;

public class Threading : BaseBuiltinStructDefinition
{
    public Threading()
        : base("threading")
    {
        base.DefineBuiltinFunction("sleep", true, SleepCurrentThread);
    }

    public override string Name => "Thread";

    private static ReturnValue SleepCurrentThread(Interpreter interpreter, IStruct self, List<Statement> statements)
    {
        var time = statements.FirstOrDefault()?.Take(interpreter);
        if (time is null)
            interpreter.Panic("expected an argument specifying the amount of milliseconds to sleep for.");

        if (time is decimal fSecs)
        {
            Thread.Sleep((int)fSecs);
        }
        else if (time is long lSecs)
        {
            Thread.Sleep((int)lSecs);
        }
        else
        {
            interpreter.Panic("invalid argument supplied, expected `int` or `float`");
        }

        return new ReturnValue(0, 0);
    }
}
