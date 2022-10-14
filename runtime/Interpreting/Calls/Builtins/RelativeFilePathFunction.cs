using DL.Interpreting.Api;
using DL.Interpreting.Exceptions;
using DL.Parser;
using DL.Parser.Production;

namespace DL.Interpreting.Calls.Builtins;

public class RelativeFilePathFunction : ICallable
{
    public string Identifier => "relative";

    public Literal Execute(ISyntaxTreeVisitor<DValue> interpreter, params Literal[] args)
    {
        // expect a single argument, the 'path'

        if (args.Length == 0)
        {
            throw new BadArgumentsException($"the function `{Identifier}` expects one argument.");
        }

        var literal = interpreter.VisitLiteral(args[0]);

        if (literal.Type != DType.String)
        {
            throw new BadArgumentsException($"the function `{Identifier}` expects the argument to be a string.");
        }

        string? path = literal.Instance as string;
        var relative = Directory.GetCurrentDirectory() + path;

        return new Literal(
            new Lexer.DToken { Type = Lexer.TokenType.String, Lexeme = relative },
            relative
            );
    }
}