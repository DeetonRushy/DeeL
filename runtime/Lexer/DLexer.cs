using Runtime.Lexer.Exceptions;

namespace Runtime.Lexer;

/// <summary>
/// This class is entirely responsible for generating a list of tokens from a DL config source file.
/// </summary>
public class DLexer
{
    /// <summary>
    /// When true, the lexer will not dispose of spaces and newlines.
    /// NOTE: when true, the parser will not be happy trying to parse
    /// all the whitespace. This is meant to be used for syntax highlighting to
    /// view the source in its actual form.
    /// </summary>
    public bool MaintainWhitespaceTokens { get; set; } = false;


    /// <summary>
    /// The source files contents in whole.
    /// </summary>
    private readonly string _contents;

    /// <summary>
    /// The resulting list of tokens after lexing.
    /// </summary>
    private readonly List<DToken> _tokens;

    /// <summary>
    /// The current lexeme.
    /// </summary>
    private string _lexeme;

    /// <summary>
    /// The current position
    /// </summary>
    private int _position;

    /// <summary>
    /// The current line number being viewed.
    /// </summary>
    private uint _line;


    /// <summary>
    /// Initialize the lexer using a string literal.
    /// </summary>
    /// <param name="contents">The source code to attempt to lex.</param>
    public DLexer(string contents)
    {
        _contents = contents + '\0';
        DConstants.Contents = _contents;
        _tokens = new List<DToken>();
        //                                 -1 is needed.
        _lexeme = string.Empty;
        _position = -1;
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
        for (; ; )
        {
            var token = LexSingle();

            // Returns null when windows newlines are detected (and possibly others).
            // This is to ignore the '\r' and to skip whitespace.
            if (token is null || (!MaintainWhitespaceTokens && token.Type == TokenType.Whitespace))
            {
                continue;
            }

            if (token.Type == TokenType.String && token.Lexeme == string.Empty)
                continue;

            if (token.Type == TokenType.Eof)
            {
                break;
            }

            if (token.Type == TokenType.Newline)
            {
                ++_line;
                if (MaintainWhitespaceTokens)
                    _tokens.Add(token);
                continue;
            }

            _tokens.Add(token);
            _lexeme = string.Empty;
        }

        _tokens.Add(new DToken { Type = TokenType.Eof });
        return _tokens;
    }
    
    private DToken? LexSingle()
    {
        // Dont inline, makes debugging near impossible.
        var ch = Advance();

        return ch switch
        {
            DConstants.ListOpen =>  MakeToken(TokenType.ListOpen),
            DConstants.ListClose => MakeToken(TokenType.ListClose),
            DConstants.DictOpen =>  MakeToken(TokenType.DictOpen),
            DConstants.DictClose => MakeToken(TokenType.DictClose),
            DConstants.Comma =>     MakeToken(TokenType.Comma),
            DConstants.LineBreak => MakeToken(TokenType.LineBreak),
            DConstants.Comment =>   LexComment(),
            DConstants.EOF =>       MakeToken(TokenType.Eof),
            DConstants.Endline =>   MakeToken(TokenType.Newline),
            DConstants.WindowsGarbage => null,
            DConstants.Whitespace => MakeToken(TokenType.Whitespace),
            DConstants.Equals => MakeToken(TokenType.Equals),
            DConstants.Colon => MakeToken(TokenType.Colon),
            DConstants.CallOpen => MakeToken(TokenType.CallOpen),
            DConstants.CallClose => MakeToken(TokenType.CallClose),
            var c when char.IsNumber(c) => LexGenericNumber(),
            var c when DConstants.IsDLIdentifierChar(c) => LexIdentifier(),
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
        ++_line;
        return MakeToken(TokenType.Comment);
    }

    private DToken LexString()
    {
        // The goal is to have *ONLY* the string contents within the lexeme.
        // Do not include the delimiters.

        /* [*] = good
         * [-] = bad
         * 
         *  'hello, world!'
         *  -*************-
         */

        // cleaner ways to write this, yes... but its impossible to debug.

        char current = Current();

        if (DConstants.IsStringDelimeter(current))
            current = Advance();

        while (!DConstants.IsStringDelimeter(current))
        {
            _lexeme += current;
            current = Advance();
        }

        Expect(!_lexeme.Any(DConstants.IsStringDelimeter),
            "failed to correctly lex string contents.");

        return MakeToken(TokenType.String, _lexeme);
    }

    private DToken LexGenericNumber()
    {
        /*
         * numbers in DL are represented as either an Int64 or Decimal.
         * All there is to do, is to collect the numbers characters & verify
         * it's correct.
         */

        var ch = Current();

        while (true)
        {
            _lexeme += ch;

            if (ch == DConstants.EOF)
            {
                throw new 
                    LexerException("unexpected end of file while lexing a number.");
            }

            if (!DConstants.IsDLNumberCharacter(Peek()))
                break;

            if (Peek() == DConstants.LineBreak || Peek() == DConstants.EOF)
                break;

            ch = Advance();
        }

        var content = _lexeme.Trim();

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

    /// <summary>
    /// For lexing an identifier. An identifier would be anything, such as boolean values
    /// or constant variables for use in the config file.
    /// </summary>
    /// <returns></returns>
    private DToken LexIdentifier()
    {
        var ch = Current();

        while (DConstants.IsDLIdentifierChar(ch))
        {
            _lexeme += ch;

            if (ch == DConstants.EOF)
            {
                throw new
                    LexerException("unexpected end of file while lexing an identifier.");
            }

            if (Peek() == DConstants.LineBreak || Peek() == DConstants.EOF || Peek() == DConstants.CallOpen)
                break;

            ch = Advance();
        }

        /*
         * For some reason the span just fucks everything here.
         * 
         * The reason the End of the span is adjusted is to account for bad lexing.
         */

        if (DConstants.BooleanValues.Contains(_lexeme))
        {
            return MakeToken(TokenType.Boolean);
        }

        if (DConstants.ReservedKeywords.ContainsKey(_lexeme))
        {
            return MakeToken(DConstants.ReservedKeywords[_lexeme]);
        }

        return (_lexeme == DConstants.Null)
            ? MakeToken(TokenType.Null)
            : MakeToken(TokenType.Identifier);
    }

    /* helpers */

    DToken MakeToken(TokenType type)
    {
        var res = new DToken ()
        {
            Lexeme = _lexeme,
            Type = type,
            Line = (int)_line
        };

        return res;
    }

    private DToken MakeToken(TokenType type, object literal)
    {
        var token = MakeToken(type);
        token.Literal = literal;
        return token;
    }

    private DToken LastToken()
    {
        return (_tokens.Count >= 1) ? _tokens[^1] : DToken.Bad;
    }

    private char Peek()
    {
        var cond = ((_position + 1) > _contents.Length);
        return cond ? DConstants.EOF : _contents[_position + 1];
    }

    private char Current()
    {
        return _contents[_position];
    }

    private char Advance()
    {
        return (++_position >= _contents.Length)
            ? DConstants.EOF
            : _contents[_position];
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition)
            throw new LexerException(message);
    }
}