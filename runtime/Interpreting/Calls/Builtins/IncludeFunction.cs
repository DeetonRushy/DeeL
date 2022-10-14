using DL.Interpreting.Api;
using DL.Interpreting.Calls;
using DL.Interpreting.Exceptions;
using DL.Lexer;
using DL.Parser;
using DL.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins;

public class IncludeFunction : ICallable
{
    /*
     * Read another file and store it into a string.
     */

    public string Identifier => "include";

    public Literal Execute(ISyntaxTreeVisitor<DValue> interpreter, params Literal[] args)
    {
        if (args.Length != 1)
        {
            throw new BadArgumentsException($"`{Identifier}` expects 1 argument.");
        }

        var fileName = interpreter.VisitLiteral(args[0]);

        if (fileName.Type != DType.String)
        {
            throw new BadArgumentsException($"`{Identifier}` the first argument to be a string.");
        }

        var path = fileName.Instance as string;

        if (!File.Exists(path))
        {
            return new Literal(DToken.MakeVar(TokenType.Boolean), false);
        }

        return new Literal(DToken.MakeVar(TokenType.String),
            File.ReadAllText(path));
    }
}