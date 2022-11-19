
using Runtime.Lexer;
using System.Text;
using Pastel;
using System.Drawing;

namespace playground;

internal class PrettyView
{
    private readonly List<DToken> tokens;

    public PrettyView(List<DToken> tokens)
    {
        this.tokens = tokens;
    }

    public string Output()
    {
        // We will take ';' as line breaks for this.
        StringBuilder sb = new StringBuilder();

        foreach (var token in tokens)
        {
            var highlighted = (token.Type, token.Lexeme, token.Literal) switch
            {
                (TokenType.String, _, _) => Highlight($"'{token.Lexeme}'", Color.Orange),
                (TokenType.Number, _, _) => Highlight($"{token.Lexeme}", Color.GreenYellow),
                (TokenType.Boolean, _, _) => Highlight($"{token.Lexeme}", Color.LightBlue),
                (TokenType.Decimal, _, _) => Highlight($"{token.Lexeme}", Color.GreenYellow),
                (TokenType.LineBreak, _, _) => ";",
                (TokenType.Equals, _, _) => "=",
                (TokenType.Whitespace, _, _) => " ",
                (TokenType.Newline, _, _) => "\n",
                (TokenType.Colon, _, _) => ":",
                (TokenType.LeftBrace, _, _) => "{",
                (TokenType.RightBrace, _, _) => "}",
                (TokenType.ListOpen, _, _) => "[",
                (TokenType.ListClose, _, _) => "]",
                (TokenType.LeftParen, _, _) => "(",
                (TokenType.RightParen, _, _) => ")",
                (TokenType.Comma, _, _) => ",",
                (TokenType.Identifier, _, _) => Highlight($"{token.Lexeme}", Color.Blue),
                (TokenType.Module, _, _) => Highlight($"mod", Color.Pink),
                (TokenType.Comment, _, _) => Highlight($"#{token.Lexeme}", Color.Gray),
                _ => token.Lexeme
            };

            sb.Append(highlighted);
        }

        return sb.ToString();
    }

    public string Highlight(string text, Color color)
        => text.Pastel(color);
}
