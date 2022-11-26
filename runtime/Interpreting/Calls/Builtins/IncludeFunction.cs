using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins;

/// <summary>
/// This function will read a file that is supplied
/// then return the contents. If the file does not exist,
/// it will return false.
/// </summary>
public class IncludeFunction : ICallable
{
    public string Identifier => "include";

    public int Arity => 1;

    public Literal Execute(Interpreter interpreter, params Literal[] args)
    {
        var fileName = interpreter.VisitLiteral(args[0]);

        if (fileName is not string path)
        {
            throw new BadArgumentsException($"`{Identifier}` the first argument to be a string.");
        }

        if (!File.Exists(path))
        {
            return new Literal(DToken.MakeVar(TokenType.String), TypeHint.String, "undefined");
        }

        return new Literal(DToken.MakeVar(TokenType.String),
            TypeHint.String,
            File.ReadAllText(path));
    }
}