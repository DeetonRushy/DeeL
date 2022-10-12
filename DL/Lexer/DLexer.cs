using DL.Lexer.Exceptions;

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
    readonly DSpan _span;

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
            // This is to ignore the '\r' and to skip whitespace.
            if (token is null || token.Type == TokenType.Whitespace)
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
            DConstants.Whitespace => MakeToken(TokenType.Whitespace),
            DConstants.Equals => MakeToken(TokenType.Equals),
            DConstants.Colon => MakeToken(TokenType.Colon),
            var c when char.IsNumber(c) => LexGenericNumber(),
            var c when DConstants.StringDelims.Contains(c) => LexString(),
            // assign the literal for debugging purposes
            _ => new DToken { Literal = ch, Type = TokenType.Invalid }
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
        Expect(DConstants.IsStringDelimeter(CurrentThenReset()), "internal error");

        // The goal is to have *ONLY* the string contents within the lexeme.
        // Do not include the delimeters.

        Skip(1); // Skip the initial delimeter

        /* [*] = good
         * [-] = bad
         * 
         * 'hello, world!'
         * -*************-
         */

        // cleaner ways to write this, yes... but its impossible to debug.

        char current = Advance();
        while (!DConstants.IsStringDelimeter(current))
        {
            current = Advance();
        }

        Expect(!_span.Contents().Any(x => DConstants.IsStringDelimeter(x)),
            "failed to correctly lex string contents.");

        return MakeToken(TokenType.String);
    }

    private DToken LexGenericNumber()
    {
        /*
         * numbers in DL are represented as either an Int64 or Decimal.
         * All there is to do, is to collect the numbers characters & verify
         * it's correct.
         */

        var ch = Advance();

        while (DConstants.IsDLNumberCharacter(ch))
        {
            if (ch == DConstants.EOF)
            {
                throw new 
                    LexerException("unexpected end of file while lexing a number.");
            }

            ch = Advance();
        }

        var content = CurrentSpan().Trim();

        bool isLong = long.TryParse(content, out var valueLong);
        bool isDecimal = decimal.TryParse(content, out var valueDecimal);

        if (!isLong && !isDecimal)
        {
            throw new LexerException($"malformed number literal: {content}");
        }

        // clear extra whitespace. EDIT: Dont work.
        // AdjustRight();

        return isLong
            ? MakeToken(TokenType.Number, valueLong)
            : MakeToken(TokenType.Decimal, valueDecimal);
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

    DToken MakeToken(TokenType type, object literal)
    {
        var token = MakeToken(type);
        token.Literal = literal;
        return token;
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

    char CurrentThenAdvance()
    {
        char current = Current();
        ++_span.End;
        return current;
    }

    char CurrentThenReset()
    {
        char current = Current();
        // discard current range.
        MakeToken(TokenType.Invalid);
        return current;
    }

    char Advance()
    {
        if (++_span.End >= _contents.Length)
            return DConstants.EOF;
        return _contents[_span.End];
    }

    void AdjustLeft(int count = 1)
    {
        if (_span.Start == _span.End)
            return;
        _span.End -= count;
        _span.Start -= count;
    }

    void AdjustRight(int count = 1)
    {
        if (_span.Start == _span.End)
            return;
        _span.Start += count;
        _span.End += count;
    }

    void Skip(int count = 1)
    {
        _span.Start += count;
    }

    void Expect(bool condition, string message)
    {
        if (!condition)
            throw new LexerException(message);
    }
}