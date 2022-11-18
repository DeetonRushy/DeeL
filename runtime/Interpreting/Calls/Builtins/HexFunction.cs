using Runtime.Interpreting.Exceptions;
using Runtime.Lexer;
using Runtime.Parser;
using Runtime.Parser.Production;

namespace Runtime.Interpreting.Calls.Builtins
{

    /// <summary>
    /// Convert a number into a hexadecimal string.
    /// </summary>
    internal class HexFunction : ICallable
    {
        public int Arity => 1;
        public string Identifier => "hex";

        public Literal Execute(Interpreter interpreter, params Literal[] args)
        {
            var s = interpreter.VisitLiteral(args.First());

            if (s is not long Value)
            {
                throw new BadArgumentsException($"expected an integer, not '{s}'.");
            }

            return new Literal(DToken.MakeVar(TokenType.String),
                "0x" + Value.ToString("X8"));
        }
    }
}
