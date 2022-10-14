using DL.Interpreting;
using DL.Lexer;
using DL.Parser;

namespace DL;

/// <summary>
/// The DL runtime. Contains methods that help to Lex, Parse and Interpret DL.
/// </summary>
public class DLRuntime
{

    public static List<DToken> ViewTokens(string contents)
    {
        return new DLexer(contents).Lex();
    }

    public static DContext ProcessConfig(string contents)
    {
        var lexer = new DLexer(contents);
        var tokens = lexer.Lex();
        var parser = new DParser(tokens);
        var ast = parser.Parse();
        var interpreter = new Interpreter();

        var config = interpreter.Interpret(ast);

        return new DContext { Config = config, Errors = parser._error.Errors };
    }

    public static DContext ProcessConfig(FileInfo contents)
    {
        var lexer = new DLexer(contents);
        var tokens = lexer.Lex();
        var parser = new DParser(tokens);
        var ast = parser.Parse();
        var interpreter = new Interpreter();

        var config = interpreter.Interpret(ast);

        return new DContext { Config = config, Errors = parser._error.Errors };
    }
}