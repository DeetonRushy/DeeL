using Runtime.Interpreting.Exceptions;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins;

/// <summary>
/// Convert a relative file path into an absolute path.
/// The path will be relative to the current directory.
/// </summary>
public class RelativeFilePathFunction : ICallable
{
    public int Arity => 1;

    public string Identifier => "relative";

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        // expect a single argument, the 'path'

        var literal = interpreter.VisitLiteral(args[0]);

        if (literal is not string path)
        {
            throw new BadArgumentsException($"the function `{Identifier}` expects the argument to be a string.");
        }

        var relative = Directory.GetCurrentDirectory() + path;

        return new Literal(
            new Lexer.DToken { Type = Lexer.TokenType.String, Lexeme = relative },
            TypeHint.String,
            relative
            );
    }
}