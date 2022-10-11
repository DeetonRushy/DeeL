using DL.Lexer;

namespace DL;

/// <summary>
/// The DL runtime. Contains methods that help to Lex, Parse and Interpret DL.
/// </summary>
public class DLRuntime
{
    private readonly DLexer lexer;

    public DLRuntime(string fileName)
    {
        lexer = new DLexer(new FileInfo(fileName));
    }

    /// <summary>
    /// Lex the contents. This will convert each `token` of the source into a <see cref="DToken"/>
    /// </summary>
    /// <returns></returns>
    public List<DToken> Lex()
        => lexer.Lex();
}