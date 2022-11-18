using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins
{
    /// <summary>
    /// This function will return the value of a specified environment variable.
    /// If the specified variable isn't present, <see cref="string.Empty"/> will
    /// be returned instead.
    /// </summary>

    internal class EnvVarFunction : ICallable
    {
        public int Arity => 1;
        public string Identifier => "envvar";

        public Literal Execute(Interpreter interpreter, params Literal[] args)
        {
            var maybeVariable = interpreter.VisitLiteral(args[0]);

            if (maybeVariable is not string variable)
            {
                throw new BadArgumentsException("environment variable names can only be a string.");
            }

            var value = Environment.GetEnvironmentVariable(variable??"");

            return new Literal(DToken.MakeVar(TokenType.String),
                value ?? string.Empty);
        }
    }
}
