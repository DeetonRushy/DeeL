using DL.Lexer.Exceptions;
using Microsoft.VisualBasic;
using System.Security.Cryptography.X509Certificates;

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
    /// The current lexeme.
    /// </summary>
    string _lexeme;

    /// <summary>
    /// The current position
    /// </summary>
    int _postion;

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
        DConstants.Contents = _contents;
        _tokens = new List<DToken>();
        //                                 -1 is needed.
        _lexeme = string.Empty;
        _postion = -1;
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

            if (token.Type == TokenType.String && token.Lexeme == string.Empty)
                continue;

            if (token.Type == TokenType.Eof)
            {
                break;
            }

            if (token.Type == TokenType.Newline)
            {
                ++_line;
                continue;
            }

            _tokens.Add(token);
            _lexeme = string.Empty;
        }

        _tokens.Add(new DToken { Type = TokenType.Eof });
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
        // Do not include the delimeters.

        /* [*] = good
         * [-] = bad
         * 
         * 'hello, world!'
         * -*************-
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

        Expect(!_lexeme.Any(x => DConstants.IsStringDelimeter(x)),
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

        while (DConstants.IsDLNumberCharacter(ch))
        {
            _lexeme += ch;

            if (ch == DConstants.EOF)
            {
                throw new 
                    LexerException("unexpected end of file while lexing a number.");
            }

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
                    LexerException("unexpected end of file while lexing a number.");
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
        return MakeToken(TokenType.Identifier);
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

    char Peek()
    {
        if ((_postion + 1) > _contents.Length)
            return DConstants.EOF;
        return _contents[_postion + 1];
    }

    char Current()
    {
        return _contents[_postion];
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
        if (++_postion >= _contents.Length)
            return DConstants.EOF;
        return _contents[_postion];
    }

    void Expect(bool condition, string message)
    {
        if (!condition)
            throw new LexerException(message);
    }
}