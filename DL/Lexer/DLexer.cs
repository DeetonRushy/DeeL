namespace DL.Lexer;

/// <summary>
/// This class is entirely responsible for generating a list of tokens from a DL config source file.
/// </summary>
public class DLexer
{
    /// <summary>
    /// The source files contents in whole.
    /// </summary>
    readonly string _contents;

    /// <summary>
    /// The resulting list of tokens after lexing.
    /// </summary>
    readonly List<DToken> _tokens;

    /// <summary>
    /// The span representing the current span of text being viewed.
    /// </summary>
    DSpan _span;

    /// <summary>
    /// The current line number being viewed.
    /// </summary>
    uint _line = 0;


    /// <summary>
    /// Initialize the lexer using a string literal.
    /// </summary>
    /// <param name="contents">The source code to attempt to lex.</param>
    public DLexer(string contents)
    {
        _contents = contents + '\0';
        DSpan.SourceContents = _contents;
        _tokens = new List<DToken>();
        //                                 -1 is needed.
        _span = new DSpan { Start = 0, End = -1 };
    }

    /// <summary>
    /// Initialize the lexer using a file. 
    /// </summary>
    /// <param name="info">The file to attempt to read the source contents from</param>
    public DLexer(FileInfo info)
        : this(File.ReadAllText(info.FullName))
    {}

    public List<DToken> Lex()
    {
        // TODO: lex the contents.

        DToken? token;

        for (; ; )
        {
            token = LexSingle();

            // Returns null when windows newlines are detected (and possibly others).
            // This is to ignore the '\r'
            if (token is null)
            {
                continue;
            }

            if (token.Type == TokenType.Eof)
            {
                break;
            }

            _tokens.Add(token);
        }

        return _tokens;
    }
    
    public DToken? LexSingle()
    {
        // Dont make inline, makes debugging near impossible.
        var ch = Advance();

        return ch switch
        {
            DConstants.ListOpen =>  MakeToken(TokenType.ListOpen),
            DConstants.ListClose => MakeToken(TokenType.ListClose),
            DConstants.DictOpen =>  MakeToken(TokenType.DictOpen),
            DConstants.DictClose => MakeToken(TokenType.DictClose),
            DConstants.Comma =>     MakeToken(TokenType.Comma),
            DConstants.Comment =>   LexComment(),
            DConstants.EOF =>       MakeToken(TokenType.Eof),
            DConstants.Endline =>   LexNewline(),
            DConstants.WindowsGarbage => null,
            DConstants.Whitespace => DToken.Whitespace,
            var c when DConstants.StringDelims.Contains(c) => LexString(),
            var c when char.IsNumber(c) => LexGenericNumber(),
            _ => DToken.Bad
        };
    }

    /// <summary>
    /// Lex a DL comment. Example: `# this is a comment\n`
    /// </summary>
    /// <returns>A Token containing a Comment.</returns>
    private DToken LexComment()
    {
        /* ignore text? would make it quicker. */

        char current;
        while ((current = Advance()) != DConstants.Endline)
        {
            if (current == DConstants.EOF)
                break;
        }

        return MakeToken(TokenType.Comment);
    }

    private DToken LexString()
    {
        // TODO: lex strings
        return DToken.Bad;
    }

    private DToken LexGenericNumber()
    {
        // TODO: lex numbers
        return DToken.Bad;
    }

    private DToken LexNewline()
    {
        ++_line;
        return MakeToken(TokenType.Newline);
    }

    /* helpers */

    DToken MakeToken(TokenType type)
    {
        var res = new DToken ()
        {
            Lexeme = new DSpan { Start = _span.Start, End = _span.End },
            Type = type,
            Line = (int)_line
        };

        _span.Start = _span.End;
        return res;
    }

    DToken LastToken()
    {
        if (_tokens.Count >= 1)
            return _tokens[^1];
        return DToken.Bad;
    }

    string CurrentSpan()
    {
        return _contents[_span.Start.._span.End];
    }

    char Current()
    {
        return _contents[_span.End];
    }

    char Advance()
    {
        if (++_span.End >= _contents.Length)
            return DConstants.EOF;
        return _contents[_span.End];
    }
}