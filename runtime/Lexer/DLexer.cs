using Microsoft.VisualBasic;
using Runtime.Lexer.Exceptions;

using static Runtime.Lexer.DConstants;

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
            case ListOpen:  
                return MakeToken(TokenType.ListOpen);
            case ListClose: 
                return MakeToken(TokenType.ListClose);
            case LeftBrace: 
                return MakeToken(TokenType.LeftBrace);
            case RightBrace: 
                return MakeToken(TokenType.RightBrace);
            case Comma: 
                return MakeToken(TokenType.Comma);
            case LineBreak: 
                return MakeToken(TokenType.LineBreak);
            case Comment: 
                return LexComment();
            case EOF: 
                return MakeToken(TokenType.Eof);
            case Endline: 
                return MakeToken(TokenType.Newline);
            case WindowsGarbage: 
                return null;
            case Whitespace: 
                return MakeToken(TokenType.Whitespace);
            case DConstants.Equals:
                {
                    if (Peek() == DConstants.Equals)
                    {
                        _ = Advance();
                        return MakeToken(TokenType.EqualComparison);
                    }
                    return MakeToken(TokenType.Equals);
                }
            case Bang:
                {
                    if (Peek() == DConstants.Equals)
                    {
                        _ = Advance();
                        return MakeToken(TokenType.NotEqual);
                    }
                    return MakeToken(TokenType.Not);
                }
            case Colon:
                {
                    if (Peek() == Colon)
                    {
                        _ = Advance();
                        return MakeToken(TokenType.Access);
                    }
                    return MakeToken(TokenType.Colon);
                }
            case LeftParen: 
                return MakeToken(TokenType.LeftParen);
            case RightParen: 
                return MakeToken(TokenType.RightParen);
            case Minus:
                {
                    if (Peek() == GreaterThan) {
                        _ = Advance();
                        return MakeToken(TokenType.Arrow);
                    }
                    return MakeToken(TokenType.Minus);
                }
            case Plus:
                return MakeToken(TokenType.Plus);
            case Multipy:
                return MakeToken(TokenType.Star);
            case Divide:
                return MakeToken(TokenType.Divide);
            case Modulo:
                return MakeToken(TokenType.Modulo);
            case GreaterThan:
                {
                    if (Peek() == DConstants.Equals)
                    {
                        _ = Advance();
                        return MakeToken(TokenType.GreaterEqual);
                    }
                    return MakeToken(TokenType.Greater);
                }
            case LesserThan:
                {
                    if (Peek() == DConstants.Equals)
                    {
                        _ = Advance();
                        return MakeToken(TokenType.LesserEqual);
                    }
                    return MakeToken(TokenType.Lesser);
                }
            case var c when char.IsNumber(c): return LexGenericNumber();
            case var c when IsDLIdentifierChar(c): return LexIdentifier();
            case var c when StringDelims.Contains(c): return LexString();
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
        while ((current = Advance()) != Endline)
        {
            if (current == EOF)
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

        if (IsStringDelimeter(current))
            current = Advance();

        while (!IsStringDelimeter(current))
        {
            _lexeme += current;
            current = Advance();
        }

        Expect(!_lexeme.Any(IsStringDelimeter),
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

            if (ch == EOF)
            {
                throw new 
                    LexerException("unexpected end of file while lexing a number.");
            }

            if (!IsDLNumberCharacter(Peek()))
                break;

            if (Peek() == LineBreak || Peek() == EOF)
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

        while (IsDLIdentifierChar(ch))
        {
            _lexeme += ch;

            if (ch == EOF)
            {
                throw new
                    LexerException("unexpected end of file while lexing an identifier.");
            }

            if (Peek() == LineBreak || Peek() == EOF || 
                Peek() is LeftParen or RightParen or Comma or Colon or LeftBrace)
                break;

            ch = Advance();
        }

        /*
         * For some reason the span just fucks everything here.
         * 
         * The reason the End of the span is adjusted is to account for bad lexing.
         */

        if (BooleanValues.Contains(_lexeme))
        {
            return MakeToken(TokenType.Boolean);
        }

        if (ReservedKeywords.ContainsKey(_lexeme))
        {
            return MakeToken(ReservedKeywords[_lexeme]);
        }

        return (_lexeme == Null)
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

    private char Peek()
    {
        var cond = ((_position + 1) > FileContents.Length);
        return cond ? EOF : FileContents[_position + 1];
    }

    private char Current()
    {
        return FileContents[_position];
    }

    private char Advance()
    {
        return (++_position >= FileContents.Length)
            ? EOF
            : FileContents[_position];
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition)
            throw new LexerException(message);
    }
}