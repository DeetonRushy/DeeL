using Runtime.Interpreting;
using Runtime.Parser.Errors;

namespace Runtime;

public class DContext
{
    public DContext(DErrorHandler errorHandler, Interpreter interpreter)
    {
        ErrorHandler = errorHandler;
        Interpreter = interpreter;
    }

    public DErrorHandler ErrorHandler { get; set; }
    public Interpreter Interpreter { get; set; }
}