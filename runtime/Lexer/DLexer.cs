using Microsoft.VisualBasic;
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
    private readonly string FileContents;

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
        FileContents = contents + '\0';
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

            if (token.Type == TokenType.Invalid)
            {
                _lexeme = "";
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

        switch (ch)
        {
            case DConstants.ListOpen:  
                return MakeToken(TokenType.ListOpen);
            case DConstants.ListClose: 
                return MakeToken(TokenType.ListClose);
            case DConstants.LeftBrace: 
                return MakeToken(TokenType.LeftBrace);
            case DConstants.RightBrace: 
                return MakeToken(TokenType.RightBrace);
            case DConstants.Comma: 
                return MakeToken(TokenType.Comma);
            case DConstants.LineBreak: 
                return MakeToken(TokenType.LineBreak);
            case DConstants.Comment: 
                return LexComment();
            case DConstants.EOF: 
                return MakeToken(TokenType.Eof);
            case DConstants.Endline: 
                return MakeToken(TokenType.Newline);
            case DConstants.WindowsGarbage: 
                return null;
            case DConstants.Whitespace: 
                return MakeToken(TokenType.Whitespace);
            case DConstants.Equals: 
                return MakeToken(TokenType.Equals);
            case DConstants.Colon: 
                return MakeToken(TokenType.Colon);
            case DConstants.LeftParen: 
                return MakeToken(TokenType.LeftParen);
            case DConstants.RightParen: 
                return MakeToken(TokenType.RightParen);
            case DConstants.Minus:
                {
                    if (Peek() == DConstants.GreaterThan) {
                        return MakeToken(TokenType.Arrow);
                    }
                    return MakeToken(TokenType.Minus);
                }
            case var c when char.IsNumber(c): return LexGenericNumber();
            case var c when DConstants.IsDLIdentifierChar(c): return LexIdentifier();
            case var c when DConstants.StringDelims.Contains(c): return LexString();
            // assign the literal for debugging purposes
            default:
                return new DToken { Literal = ch, Type = TokenType.Invalid };
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

            if (Peek() == DConstants.LineBreak || Peek() == DConstants.EOF || 
                Peek() is DConstants.LeftParen or DConstants.RightParen or DConstants.Comma or DConstants.Colon or DConstants.LeftBrace)
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
        var cond = ((_position + 1) > FileContents.Length);
        return cond ? DConstants.EOF : FileContents[_position + 1];
    }

    private char Current()
    {
        return FileContents[_position];
    }

    private char Advance()
    {
        return (++_position >= FileContents.Length)
            ? DConstants.EOF
            : FileContents[_position];
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition)
            throw new LexerException(message);
    }
}