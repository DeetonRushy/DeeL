using Microsoft.VisualBasic;
using Runtime.Interpreting;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Errors;

namespace Runtime;

/// <summary>
/// The DL runtime. Contains methods that help to Lex, Parse and Interpret DL.
/// </summary>
public class DlRuntime
{

    public static List<DToken> ViewTokens(string contents)
    {
        return new DLexer(contents).Lex();
    }

    public static DContext Run(string source, bool except)
    {
        var lexer = new DLexer(source);
        var tokens = lexer.Lex();
        var parser = new DParser(tokens, source.Split('\n').ToList());
        var interpreter = new Interpreter();

        try
        {
            var ast = parser.Parse();
            interpreter.Interpret(ast);
        }
        catch (Exception ex)
        {
            if (except)
                Console.WriteLine("exception: {0}", ex.Message);
            else
                throw;
        }

        return new DContext(parser.Errors, interpreter);
    }
}