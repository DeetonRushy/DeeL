using Runtime.Interpreting;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Errors;
using System.Diagnostics;

namespace Runtime;

/// <summary>
/// The DL runtime. Contains methods that help to Lex, Parse and Interpret DL.
/// </summary>
public class DlRuntime
{
    public static string Version { get; } = "0.3";

    public static List<DToken> ViewTokens(string contents)
    {
        return new DLexer(contents).Lex();
    }

    public static DContext Run(string source, bool except)
    {
        DErrorHandler.SourceLines = source.Split('\n').ToList();

        var lexer = new DLexer(source);
        var tokens = lexer.Lex();
        var parser = new DParser(tokens);
        Interpreter? interpreter = null;

        try
        {
            var ast = parser.Parse();
            interpreter = new Interpreter(ast);
            var timeTakenToInterpret = Stopwatch.StartNew();
            interpreter.Interpret();
            timeTakenToInterpret.Stop();
            Console.WriteLine($"Took {timeTakenToInterpret.ElapsedMilliseconds}ms to interpret.");
        }
        catch (Exception ex)
        {
            if (except)
                Console.WriteLine("exception: {0}", ex.Message);
            else
                throw;
        }

        return new DContext(parser.Errors, interpreter!);
    }
}