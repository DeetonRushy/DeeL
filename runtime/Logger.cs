using System.Runtime.CompilerServices;

namespace Runtime;

public class Logger
{
    private static bool ShouldLog
        => Configuration.GetFlag("debug");

    public static void Info
        (object self, string message,
            [CallerMemberName] string caller = "",
            [CallerLineNumber] int line = 0)
    {
        if (!ShouldLog) return;
        Console.WriteLine($"(info)[{caller}:{line} ({self})] {message}");
    }
    
    public static void Err
    (object self, string message,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int line = 0)
    {
        if (!ShouldLog) return;
        Console.WriteLine($"(error)[{caller}:{line} ({self})] {message}");
    }
}