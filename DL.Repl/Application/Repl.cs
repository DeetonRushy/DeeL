using System.Text;
using Runtime.Interpreting;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace DL.Repl.Application;

// The REPL will not be for designing large objects.
// Just jump in & test shit.

public class Repl
{
    private const string QuitLiteral = "_quit";
    
    public Repl()
    {
        _interpreter = new Interpreter(Array.Empty<Statement>().ToList()) { PreventExit = true };
        _isInFunction = false;
    }
    
    // Use the same interpreter instance to save state.
    private readonly Interpreter _interpreter;
    private bool _isInFunction;
    
    public void Execute()
    {
        WriteInitialOutput();
        var sb = new StringBuilder();
        
        while (true)
        {
            ConsoleKeyInfo keyInfo;
            // pressing [esc] is a forced exit basically
            while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Escape)
            {
                if (!_isInFunction && sb.ToString().Contains("fn"))
                {
                    _isInFunction = true;
                }
                
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (!_isInFunction || sb.ToString().Contains("}"))
                        break;
                    Console.Write("\n>> ");
                    continue;
                }

                sb.Append(keyInfo.KeyChar);
                Console.Write(keyInfo.KeyChar);
            }

            var script = sb.ToString();
            if (script == QuitLiteral)
                break;
            ExecuteExpression(script);
        }
    }

    private void ExecuteExpression(string contents)
    {
        try
        {
            var tokens = new DLexer(contents).Lex();
            var ast = new DParser(tokens).Parse();

            _interpreter.ReloadAst(ast);
            _interpreter.Interpret();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"internal exception occured: {ex}");
        }
    }

    private void WriteInitialOutput()
    {
        Console.WriteLine("DL REPL -- (Work in progress)");
        Console.WriteLine("[https://github.com/deetonrushy/DeeL]\n");
    }
}